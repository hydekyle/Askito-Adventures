using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public Transform itemsT;
    List<Transform> itemsList = new List<Transform>();
    int lastIndex = -1;

    public Sprite soldSprite;

    private void Start()
    {
        foreach (Transform t in itemsT) itemsList.Add(t);
    }

    public void ItemTouched(int itemNumber)
    {
        if (lastIndex == itemNumber) BuyItem(itemNumber);

        HideLastOne();

        Animator itemAnimator = itemsList[itemNumber].GetComponent<Animator>();
        ShowPrice(itemAnimator);

        lastIndex = itemNumber;
    }

    void HideLastOne()
    {
        if (lastIndex >= 0) itemsList[lastIndex].GetComponent<Animator>().Play("HidePrice");
    }

    void ShowPrice(Animator itemAnimator)
    {
        if (itemAnimator.GetCurrentAnimatorStateInfo(0).IsName("ShowPrice")) itemAnimator.Play("HidePrice");
        else itemAnimator.Play("ShowPrice");
    }

    void MakeItemGrey(int itemNumber)
    {
        Transform slotT = itemsList[itemNumber].Find("Slot");
        itemsList[itemNumber].Find("Image").GetComponent<Image>().enabled = false;
        slotT.GetComponent<Button>().enabled = false;
        slotT.GetComponent<Image>().sprite = soldSprite;
    }

    void BuyItem(int itemNumber)
    {
        MakeItemGrey(itemNumber);
    }

}
