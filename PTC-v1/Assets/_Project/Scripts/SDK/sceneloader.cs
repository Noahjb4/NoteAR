using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneloader : MonoBehaviour
{
    public void SceneStart() { SceneManager.LoadScene("Start"); }
    public void MarsTest() { SceneManager.LoadScene("MarsTestScene"); }
    public void AnchorBasic() { SceneManager.LoadScene("AzureSpatialAnchorsBasicDemo"); }
    public void AnchorNearby() { SceneManager.LoadScene("AzureSpatialAnchorsNearbyDemo"); }
    public void AnchorLocalShared() { SceneManager.LoadScene("AzureSpatialAnchorsLocalSharedDemo"); }
    public void AnchorCoarseReloc() { SceneManager.LoadScene("AzureSpatialAnchorsCoarseRelocDemo"); }
    public void BlobImage() { SceneManager.LoadScene("ImageScene"); }
    public void BlobAudio() { SceneManager.LoadScene("AudioScene"); }
    public void BlobText() { SceneManager.LoadScene("TextScene"); }
    public void BlobList() { SceneManager.LoadScene("ListScene"); }
}
