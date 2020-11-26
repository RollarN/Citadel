

#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
//[ExecuteInEditMode]
public class SubsceneRoot : MonoBehaviour
{
    public const string k_SubsceneRootTag = "SubsceneRoot";

    /// Button displayed via the SubSceneRootEditor script
    private string m_PathToAsset = null;
    private const string k_CheckedOutLocalName = " (CO Local)";
    private const string k_CheckedOutRemoteName = " (CO Remote)";
    private bool m_CheckedOutLocal = false;
    private bool m_CheckedOutRemote = false;
    private bool m_CheckOverrides = false;
    private readonly WaitForSeconds m_OneSecond = new WaitForSeconds(1);
    private readonly WaitForSeconds m_FiveSeconds = new WaitForSeconds(5);
    private readonly WaitForSeconds m_ThirtySeconds = new WaitForSeconds(30);
    private bool m_CheckRunning = false;
    private float m_LastNameCheck = 0f;
    private const float m_TimeBetweenNameChecks = 2f;

    private void OnEnable() 
    {
        gameObject.tag = k_SubsceneRootTag;
        m_PathToAsset = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
        m_LastNameCheck = Time.time;
        SceneView.duringSceneGui += OnScene;
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            enabled = false;
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    private void OnScene(SceneView scene)
    {
        if(Event.current.isMouse || Event.current.isKey || Event.current.isScrollWheel)
        {
            m_LastNameCheck = Time.time;
            return;
        }
        else
        {
            if(Time.time - m_LastNameCheck > m_TimeBetweenNameChecks)
            {
                UpdateName();
                m_LastNameCheck = Time.time;
            }
        }

    }

    public void CheckForPrefabOverrides()
    {
        if (!m_CheckRunning)
        {
            m_CheckRunning = true;
            StartCoroutine(DoCheck());
        }
    }

    private IEnumerator DoCheck()
    {
        yield return m_OneSecond;
        if (!Application.isPlaying && PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            yield return null;
            if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
            {
                yield return null;
                if (ApplyComponents() || PrefabUtility.HasPrefabInstanceAnyOverrides(gameObject, false))
                {
                    yield return null;
                    m_PathToAsset = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);

                    yield return null;
                    if (CustomPrefabUtility.CheckOutPrefabInstanceIfValid(gameObject, m_PathToAsset))
                    {
                        yield return null;
                        PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
                    }
                }
            }
        }
        m_CheckRunning = false;
    }


    private void UpdateName()
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            return;
        }

        m_PathToAsset = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
        Task statusTask = Provider.Status(m_PathToAsset);
        statusTask.Wait();

        if (CustomPrefabUtility.IsPrefabCheckedoutLocal(ref statusTask))
        {
            if (!m_CheckedOutLocal)
            {
                gameObject.name.Replace(k_CheckedOutRemoteName, k_CheckedOutLocalName);
                if (!gameObject.name.Contains(k_CheckedOutLocalName))
                {
                    gameObject.name = gameObject.name + k_CheckedOutLocalName;
                }

                m_CheckedOutLocal = true;
            }
        }
        else
        {
            if (gameObject.name.Contains(k_CheckedOutLocalName))
            {
                gameObject.name = gameObject.name.Replace(k_CheckedOutLocalName, string.Empty);
            }
            m_CheckedOutLocal = false;
        }
        if (CustomPrefabUtility.IsPrefabCheckedOutRemote(statusTask))
        {
            if (!m_CheckedOutRemote)
            {
                gameObject.name.Replace(k_CheckedOutRemoteName, k_CheckedOutLocalName);
                if (!gameObject.name.Contains(k_CheckedOutRemoteName))
                {
                    gameObject.name = gameObject.name + k_CheckedOutRemoteName;
                }
                m_CheckedOutRemote = true;
            }
        }
        else
        {
            m_CheckedOutRemote = false;

            if (gameObject.name.Contains(k_CheckedOutRemoteName))
            {
                gameObject.name = gameObject.name.Replace(k_CheckedOutRemoteName, string.Empty);
            }
        }
    
    }

    private bool ApplyComponents()
    {
        bool apply = PrefabUtility.GetAddedComponents(gameObject).Count > 0
                        || PrefabUtility.GetRemovedComponents(gameObject).Count > 0
                        || PrefabUtility.GetAddedGameObjects(gameObject).Count > 0;

        if (apply)
        {
            PrefabUtility.GetAddedComponents(gameObject).ForEach(component => component.Apply());
            PrefabUtility.GetRemovedComponents(gameObject).FindAll(component => component.assetComponent != null).ForEach(component => component.Apply());
            PrefabUtility.GetAddedGameObjects(gameObject).ForEach(obj => obj.Apply());
            Debug.Log(gameObject.name + " Apply was true");
            return true;
        }
        return false;
    }
}

#endif