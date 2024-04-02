// An ItemGetScreen appears when the player picks up an ItemObject.
// SmallItemGetScreen shows a small image of the Item the player just picked up, corresponding to the parameters set in ItemObject.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SmallItemGetScreen : MonoBehaviour
{
    public Image _icon;

    public void Set(UseableItem item)
    {
        var item_prefab = (GameObject)Resources.Load("Inventory/Useable/" + item._name);
        _icon.sprite = item_prefab.GetComponent<Image>().sprite;
        _icon.SetNativeSize();

        StartCoroutine(End());
    }

    public void Set(Artifact item)
    {
        var item_prefab = (GameObject)Resources.Load("Inventory/Artifact/" + item._name);
        _icon.sprite = item_prefab.GetComponent<Image>().sprite;
        _icon.SetNativeSize();

        StartCoroutine(End());
    }

    private IEnumerator End()
    {
        SoundEffectSystem.Play(SFX.gain_sp);
        for (float time = 0; time <= 1.5f; time += Time.deltaTime) { yield return null; }
        Destroy(gameObject);
    }
}
