using UnityEngine;
using System.Collections;

/// <summary>
/// サウンドマネージャ
/// </summary>
public class Game_SoundManager : MonoBehaviour
{
    public static Game_SoundManager Instance
    {
        get { return instance; }
    }

    static Game_SoundManager instance;

    public AudioSource put;
    public AudioSource turn;

    void Awake()
    {
        instance = this;
    }
}
