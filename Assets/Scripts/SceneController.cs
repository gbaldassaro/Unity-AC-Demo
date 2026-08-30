using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController singleton;

    void Start()
    {
        singleton = this;
        DontDestroyOnLoad(gameObject); 
    } 

    public void LoadScene(string name)
    {
        StartCoroutine(Load(name));
    }

    public void ReloadScene()
    {
        string active = SceneManager.GetActiveScene().name;
        StartCoroutine(Load(active));
    }

    private IEnumerator Load(string name)
    {
        AsyncOperation baseAsyncLoad = SceneManager.LoadSceneAsync("Base Scene");

        while (!baseAsyncLoad.isDone)
        {
            yield return null;
        }
        
        yield return new WaitForEndOfFrame();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
    }
}
