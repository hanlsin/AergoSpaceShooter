using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using aergo.heracs;


public class GameController : MonoBehaviour
{
    public GameObject hazard;
    public Vector3 spawnValue;
    public int hazardCount;
    public float spawnWait;
    public float startWait;
    public float waveWait;

    public Text scoreText;
    private string scoreFmt = "{0:D10}";
    private int score;

    public Text playTimeText;
    private string playTimeFmt = "{0:00}:{1:00}:{2:00}.{3:000}";
    private float playTime;

    public Text fpsText;
    public float fpsRefreshTime = 0.5f;
    private float deltaTime = 0.0f;
    private float refreshWaitTime = 0.0f;

    public Text restartText;
    public Text gameOverText;
    private string gameOverFmt = "Game Over!\nYour Score: {0:D10}\nYour Play Tme: {1}";
    private bool isGameOver = false;
    private bool isRestart = false;

    public InputField nameInputField;
    public Button sendButton;
    public TMPro.TMP_Text scoreBoardText;

    private Aergo aergo;
    private const string SC_ADDRESS = "AmhS8UjT3GCr2XZA8mQyPGXM7Ne922GQmrQg3Zp6Ji9apTjKc8iB";
    private bool sentScore = false;
    private bool refreshingScoreBoard = false;

    private class Score
    {
        public string user_id { get; set; }
        public ulong score { get; set; }
        public string playtime { get; set; }
        public ulong block_no { get; set; }
        public string tx_id { get; set; }
    }

    private class ScoreList
    {
        public string __module { get; set; }
        public string __func_name { get; set; }
        public int __status_code { get; set; }
        public string __status_sub_code { get; set; }
        public string __err_msg { get; set; }
        public IList<Score> score_list { get; set; }
    }

    private void Start()
    {
        UpdateScore();
        StartCoroutine(SpawnWaves());

        restartText.gameObject.SetActive(false);
        gameOverText.text = "";
        nameInputField.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(false);
        
        scoreBoardText.gameObject.SetActive(false);
    }

    private IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(startWait);
        while (true)
        {
            for (int i = 0; i < hazardCount; i++)
            {
                Vector3 spawnPosition = new Vector3(Random.Range(-spawnValue.x, spawnValue.x), spawnValue.y, spawnValue.z);
                Quaternion spawnRotation = Quaternion.identity;
                Instantiate(hazard, spawnPosition, spawnRotation);
                yield return new WaitForSeconds(spawnWait);
            }

            yield return new WaitForSeconds(waveWait);

            if (isGameOver)
            {
                restartText.gameObject.SetActive(true);
                isRestart = true;
                StartCoroutine(GetScoreBoard());
                break;
            }
        }
    }

    private void UpdateScore()
    {
        scoreText.text = string.Format(scoreFmt, score);
    }

    public void AddScore(int newScoreValue)
    {
        if (isGameOver)
        {
            return;
        }

        score += newScoreValue;
        UpdateScore();
    }

    private IEnumerator GetScoreBoard()
    {
        refreshingScoreBoard = true;
        restartText.gameObject.SetActive(false);

        if (aergo == null)
        {
            aergo = new Aergo();
            aergo.Connect("localhost:7845");
            aergo.NewAccount("6hXe7VLPrFLGBAsMBW3QwammXDq5w6AvRQmdPYmrWDVcjbBsSiC");
        }
        else
        {
            yield return new WaitForSeconds(3);
        }

        scoreBoardText.gameObject.SetActive(false);
        if (!sentScore)
        {
            // show input after connecting
            nameInputField.gameObject.SetActive(true);
            sendButton.gameObject.SetActive(true);
        }

        // get top 10 scores
        ArrayList args = new ArrayList();
        args.Add("__SPACE_SHOOTER__");
        args.Add("getTopScores");
        args.Add(20);
        ScoreList result = aergo.QuerySmartContract<ScoreList>(SC_ADDRESS, "callFunction", args);
        string scoreBoard = "";
        if (result.__status_code / 200 != 1)
        {
            Debug.Log(result.__err_msg);
        }
        else
        {
            scoreBoardText.gameObject.SetActive(true);
            int idx = 0;
            foreach (var s in result.score_list)
            {
                idx++;
                var u = s.user_id;
                if (s.user_id.Length > 10)
                {
                    u = s.user_id.Substring(0, 7) + "...";
                }
                scoreBoard += string.Format("{3, -2}. {0, -10} {1, 0:D10} {2}\n", u, s.score, s.playtime, idx);
            }
            Debug.Log(scoreBoard);
        }
        scoreBoardText.text = scoreBoard;
        refreshingScoreBoard = false;
        restartText.gameObject.SetActive(true);
        yield return null;
    }

    public void GameOver()
    {
        int rest = (int)playTime % 1000;
        int sec = (int)(playTime / 1000) % 60;
        int min = (int)(playTime / (1000 * 60)) % 60;
        int hour = (int)(playTime / (1000 * 60 * 60)) % 24;
        string playtime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", hour, min, sec, rest);
        gameOverText.text = string.Format(gameOverFmt, score, playtime);
        isGameOver = true;
    }

    public void SendScore()
    {
        // save score and playtime in the blockchain
        ArrayList args = new ArrayList();
        args.Add("__SPACE_SHOOTER__");
        args.Add("addScore");
        args.Add(nameInputField.text);
        args.Add(score);
        args.Add(playTimeText.text);
        var result = aergo.CallSmartContract(SC_ADDRESS, "callFunction", args, 0);
        Debug.Log(result);

        // show input after connecting
        nameInputField.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(false);
        sentScore = true;
        StartCoroutine(GetScoreBoard());
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    private void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        if (isRestart)
        {
            if (refreshingScoreBoard)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void OnGUI()
    {
        float msec = deltaTime * 1000.0f;
        if (refreshWaitTime > fpsRefreshTime)
        {
            float fps = 1.0f / deltaTime;
            fpsText.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            refreshWaitTime = 0.0f;
        }
        else
        {
            refreshWaitTime += deltaTime;
        }

        if (isGameOver)
        {
            return;
        }

        playTime += msec;
        int rest = (int)playTime % 1000;
        int sec = (int)(playTime / 1000) % 60;
        int min = (int)(playTime / (1000 * 60)) % 60;
        int hour = (int)(playTime / (1000 * 60 * 60)) % 24;
        playTimeText.text = string.Format(playTimeFmt, hour, min, sec, rest);
    }
}