using UnityEngine;

public class SimpleGridLayout : MonoBehaviour
{
    public int columns = 4; // Number of columns in the grid
    public float cellWidth = 1.0f; // Width of each cell
    public float cellHeight = 1.0f; // Height of each cell

    void Start()
    {
        ArrangeChildren();
    }

    public void ArrangeChildren()
    {
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            int row = i / columns;
            int column = i % columns;

            Vector3 newPosition = new Vector3(column * cellWidth, -row * cellHeight, 0);
            child.localPosition = newPosition;
        }
    }
}
