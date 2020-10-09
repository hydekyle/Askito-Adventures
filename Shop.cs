using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public Animator bannerAnim;
    public Transform itemsT;
    public Text textDisplayer;
    List<Transform> itemsList = new List<Transform>();
    int lastIndex = -1;

    string welcomeS = "Bienvenido a la pambishop. Solo aceptamos pambialmas como moneda de cambio.";

    List<string> descriptions = new List<string>()
    {
        "Zumo de naranja 100% natural. Bébetelo o tíraselo a la cara de alguien.",
        "Un frasco de pambitoxicidad concentrada, ten cuidado no se te caiga.",
        "Una bomba de mecha. Mechero no incluido.",
        "Una espada decente y no esa mierda que llevas contigo.",
        "Una llave misteriosa pero que probablemente no necesites para nada."
    };

    public Sprite soldSprite;

    private void Initialize()
    {
        foreach (Transform t in itemsT) itemsList.Add(t);
    }

    private void OnEnable()
    {
        if (itemsList.Count == 0) Initialize();
        foreach (Transform t in itemsT) t.GetComponent<Animator>().Play("HidePrice");
        textDisplayer.text = welcomeS;
        lastIndex = -1;
        bannerAnim.Play("BannerEnter");
    }

    private void OnDisable()
    {
        print("Pero bueno");
        HideLastOne();
    }

    public void ItemTouched(int itemNumber)
    {
        if (lastIndex == itemNumber)
        {
            BuyItem(itemNumber);
            return;
        }

        HideLastOne();
        ShowItemInfo(itemNumber);

        lastIndex = itemNumber;
    }

    void HideLastOne()
    {
        if (lastIndex >= 0) itemsList[lastIndex].GetComponent<Animator>().Play("HidePrice");
    }

    void ShowItemInfo(int itemNumber)
    {
        textDisplayer.text = descriptions[itemNumber];
        PlaySlotAnim(itemNumber);
    }

    void PlaySlotAnim(int itemNumber)
    {
        Animator itemAnimator = itemsList[itemNumber].GetComponent<Animator>();
        if (itemAnimator.GetCurrentAnimatorStateInfo(0).IsName("ShowPrice")) itemAnimator.Play("HidePrice");
        else itemAnimator.Play("ShowPrice");
    }

    void MakeItemGrey(int itemNumber)
    {
        Transform slotT = itemsList[itemNumber].Find("Slot");
        itemsList[itemNumber].Find("Image").GetComponent<Image>().enabled = false;
        slotT.GetComponent<Button>().enabled = false;
        slotT.GetComponent<Image>().sprite = soldSprite;
        PlaySlotAnim(itemNumber);
    }

    void BuyItem(int itemNumber)
    {
        MakeItemGrey(itemNumber);
        textDisplayer.text = "Órale gracias por su compra.";
    }

}
