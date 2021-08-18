using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // [Header("Gameplay")]
    // [SerializeField] private Player player;

    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        ///player = FindObjectOfType<Player>();
       // player.OnPlayerDied += OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        Invoke("ReloadScene", 3f);

    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
