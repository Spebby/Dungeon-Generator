using System;
using CMPM146.Core;
using TMPro;
using UnityEngine;


namespace CMPM146.UI {
    public class RewardScreenManager : MonoBehaviour {
        public GameObject rewardUI;

        public TextMeshProUGUI label;


        void Update() {
            switch (GameManager.Instance.State) {
                case GameManager.GameState.GAMEOVER:
                    rewardUI.SetActive(true);
                    label.text = "ENEMIES WIN";
                    gameObject.SetActive(false);
                    break;
                case GameManager.GameState.PLAYERWIN:
                    rewardUI.SetActive(true);
                    label.text = "PLAYER WINS";
                    gameObject.SetActive(false);
                    break;
                case GameManager.GameState.PREGAME:
                case GameManager.GameState.INWAVE:
                case GameManager.GameState.WAVEEND:
                case GameManager.GameState.COUNTDOWN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}