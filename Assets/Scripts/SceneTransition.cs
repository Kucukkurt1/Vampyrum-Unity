using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Sahne Ayarlarý")]
    [SerializeField] private string targetSceneName = "Corridor_1";

    [Header("Geçiþ Sonrasý Doðma Noktasý")]
    [Tooltip("Yeni sahnede karakterin belireceði boþ objenin tam adý (Örn: Spawn_From_PlayerRoom)")]
    [SerializeField] private string targetSpawnPointName;

    // Sahneler arasý taþýnan hedef nokta ismi
    public static string NextSpawnPointName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hedef noktayý statik deðiþkene yaz
            NextSpawnPointName = targetSpawnPointName;

            // Sahneyi yükle
            SceneManager.LoadScene(targetSceneName);
        }
    }
}