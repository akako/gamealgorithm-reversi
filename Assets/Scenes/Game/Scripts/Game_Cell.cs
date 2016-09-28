using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// マス目クラス
/// </summary>
[RequireComponent(typeof(Button))]
public class Game_Cell : UIBehaviour
{
    /// <summary>
    /// クリック可能か否か
    /// </summary>
    /// <value><c>true</c> if this instance is clickable; otherwise, <c>false</c>.</value>
    public bool IsClickable
    {
        set
        {
            frame.color = value ? Color.red : Color.black;
            button.enabled = value; 
        }
        get
        {
            return button.enabled;
        }
    }

    /// <summary>
    /// マスX座標
    /// </summary>
    /// <value>The x.</value>
    public int X { get { return x; } }

    /// <summary>
    /// マスY座標
    /// </summary>
    /// <value>The y.</value>
    public int Y { get { return y; } }

    /// <summary>
    /// マスに配置されている石の色
    /// </summary>
    /// <value>The color of the stone.</value>
    public Game_Field.StoneColor StoneColor
    {
        set
        {
            switch (value)
            {
                case Game_Field.StoneColor.None:
                    stone.gameObject.SetActive(false);
                    break;
                case Game_Field.StoneColor.Black:
                    stone.gameObject.SetActive(true);
                    stone.color = Color.black;
                    break;
                case Game_Field.StoneColor.White:
                    stone.gameObject.SetActive(true);
                    stone.color = Color.white;
                    break;
            }
        }
        get
        {
            if (!stone.gameObject.activeSelf)
            {
                return Game_Field.StoneColor.None;
            }
            return stone.color == Color.black ? Game_Field.StoneColor.Black : Game_Field.StoneColor.White;
        }
    }

    [SerializeField]
    Image frame;
    [SerializeField]
    Image stone;

    int x;
    int y;
    Button button;

    protected override void Awake()
    {
        base.Awake();
        button = GetComponent<Button>();
    }

    protected override void Start()
    {
        base.Start();
        button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// マスをイニシャライズします
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// マスがクリックされた際の処理
    /// </summary>
    void OnClick()
    {
        Game_SceneController.Instance.OnCellClick(this);
    }
}
