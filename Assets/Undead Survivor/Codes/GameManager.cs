using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f;

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = {3, 5, 10, 100, 150, 210, 280, 360, 450, 600};

    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject enemyCleaner;

    //랭크서버로 보낼 유저이름
    String rankUserName;

    void Awake()
    {
        instance = this;
    }

    public void GameStart(int id)
    {
        health = maxHealth;
        playerId = id;

        player.gameObject.SetActive(true);
        //임시스크립트
        uiLevelUp.Select(playerId % 2);
        Resume();

        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutaine());
    }

    IEnumerator GameOverRoutaine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();

        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutaine());
    }

    IEnumerator GameVictoryRoutaine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Win();

        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }

    public void RankSave()
    {
        rankUserName = uiResult.gameObject.transform.Find("RankUserName").Find("UserName").GetComponent<Text>().text;

        if(rankUserName == "")
        {
            uiResult.gameObject.transform.Find("RankUserName").Find("Placeholder").GetComponent<Text>().text = "이름을 작성해주세요.";
        }
        else
        {
            StartCoroutine(RankSaveRoutaine());
        }
    }

    IEnumerator RankSaveRoutaine()
    {
        string url = "https://gamehub.dait.co.kr/api/play";
        WWWForm form = new WWWForm();
        string gameId = "3";
        string gameApikey = "a2b9738f6dca5ebae83f2d8bff9f070396cee8d631f0f123ab309a055234577144bf2e038ef9886284035b93cfbec5c07cb6383914988191376b7710ff41a05d";
        string guid = Guid.NewGuid().ToString();

        form.AddField("gameId", gameId);
        form.AddField("gameApikey", gameApikey);
        form.AddField("playerName", rankUserName);
        form.AddField("playerComment", "테스트");
        form.AddField("playScore", Math.Floor(kill * (health / maxHealth)).ToString());
        form.AddField("playUuid", guid);
        form.AddField("etcText", "etc 테스트");
        using UnityWebRequest www = UnityWebRequest.Post(url, form);  // 보낼 주소와 데이터 입력

        yield return www.SendWebRequest();  // 응답 대기

        if (www.error == null)
        {
            Debug.Log(www.downloadHandler.text);    // 데이터 출력
            uiResult.gameObject.transform.Find("RankUserName").Find("UserName").gameObject.SetActive(false);
            uiResult.gameObject.transform.Find("RankUserName").Find("Done").gameObject.SetActive(true);
            uiResult.gameObject.transform.Find("Rank").gameObject.SetActive(false);
            uiResult.gameObject.transform.Find("Retry").localPosition = new Vector3(0, -40, 0);
        }
        else
        {
            Debug.Log("error");
        }

        www.Dispose();
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (!isLive) { return; }

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }

    public void GetExp()
    {
        if (!isLive) { return; }

        exp++;

        if(exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;
            uiLevelUp.Show();
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }
}
