using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Skeleton : MonoBehaviour
{
    int interactions = 0;
    int playerSouls;

    private void Start()
    {
        playerSouls = GameManager.Instance.db.GetSouls();
    }

    public void OnPlayerInteraction()
    {
        Talk();
    }

    private void Talk()
    {
        if (playerSouls == 0)
        {
            Dialog("...");
            return;
        }
        switch (interactions)
        {
            case 0: Dialog("¡Hola, churrita!", "Esas pambialmas que llevas contigo huelen muy bien."); break;
            case 1: Dialog("Antaño los pambis me daban de comer, pero se aburrieron de mí y ahora estoy literalmente en los huesos, amigo."); break;
            default: Dialog("Yo ya no tengo fuerzas, pero ahí fuera está plagado de pambis sedientos de sangre.", "Askito, eres el único que puede parar esta pandemia.", "Sal ahí fuera y farmea almas de pambis. Tráelas y podremos abrir esta puerta misteriosa. ¿Te parece?"); break;
        }

        interactions++;
    }

    private void Dialog(string text)
    {
        GameManager.Instance.ShowDialog(text);
    }

    private void Dialog(string text1, string text2)
    {
        string[] texts = new string[2];
        texts[0] = text1;
        texts[1] = text2;
        GameManager.Instance.ShowDialog(texts);
    }

    private void Dialog(string text1, string text2, string text3)
    {
        string[] texts = new string[3];
        texts[0] = text1;
        texts[1] = text2;
        texts[2] = text3;
        GameManager.Instance.ShowDialog(texts);
    }
}
