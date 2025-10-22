using UnityEngine;

public class GridManager : MonoBehaviour
{
    public void PotionClicked(Potion potion)
    {
        Debug.Log("Potion clicked in GridManager: " + potion.potionType.ToString());
        // This is where you’ll later add logic to handle swapping, matching, etc.
    }
}
