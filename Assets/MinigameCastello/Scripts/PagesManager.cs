using UnityEngine;

public class Book : MonoBehaviour
{
    public GameObject[] pageObjects;
    public GameObject writeThoughts;
    private bool isBookOpen = false;

    void Start()
    {
        HideAllPages();
        pageObjects[0].SetActive(true);
    }

    public void OpenBook()
    {
        isBookOpen = true;
        pageObjects[0].SetActive(true);
    }

    public void PreviousPage()
    {
        if (isBookOpen)
        {
            for (int i = 0; i < pageObjects.Length; i++)
            {
                if (pageObjects[i].activeSelf)
                {
                    pageObjects[i].SetActive(false);

                    if (i > 0)
                    {
                        pageObjects[i - 1].SetActive(true);
                    }
                    break;
                }
            }
        }
    }

    public void NextPage()
    {
        if (isBookOpen)
        {
            for (int i = 0; i < pageObjects.Length; i++)
            {
                if (pageObjects[i].activeSelf)
                {
                    pageObjects[i].SetActive(false);

                    if (i < pageObjects.Length - 1)
                    {
                        pageObjects[i + 1].SetActive(true);
                    }
                    break;
                }
            }
        }
    }

    public void WriteUI()
    {
        writeThoughts.SetActive(true);
    }

    public void WriteThoughts()
    {
        writeThoughts.SetActive(false);
    }

    void HideAllPages()
    {
        foreach (GameObject pageObject in pageObjects)
        {
            pageObject.SetActive(false);
        }
    }

}
