using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// メッセージ表示クラス
/// </summary>
[RequireComponent(typeof(Button))]
public class Game_Message : UIBehaviour
{
    [SerializeField]
    Text text;

    protected override void Start()
    {
        base.Start();
        GetComponent<Button>().onClick.AddListener(OnClick);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// メッセージを表示するコルーチンです
    /// </summary>
    /// <param name="message">Message.</param>
    public IEnumerator Show(string message)
    {
        text.text = message;
        gameObject.SetActive(true);
        // メッセージをクリックして消えるまでWait
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// メッセージクリック時の処理です
    /// </summary>
    void OnClick()
    {
        Game_SoundManager.Instance.put.Play();
        gameObject.SetActive(false);
    }
}
