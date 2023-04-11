#region Includes
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion


public class UINeuralNetworkPanel : MonoBehaviour
{
    #region Members
    [SerializeField]
    private List<UINeuralNetworkLayerPanel> Layers;
    #endregion

    #region Methods
    public void Display(NeuralNetwork neuralNet)
    {
        UINeuralNetworkLayerPanel dummyLayer = Layers[0];

        for (int i = Layers.Count; i < neuralNet.Layers.Length + 1; i++)
        {
            UINeuralNetworkLayerPanel newPanel = Instantiate(dummyLayer);
            newPanel.transform.SetParent(this.transform, false);
            Layers.Add(newPanel);
        }

        for (int i = this.Layers.Count-1; i >= neuralNet.Layers.Length + 1; i++)
        {
            UINeuralNetworkLayerPanel toBeDestroyed = Layers[i];
            Layers.RemoveAt(i);
            Destroy(toBeDestroyed);
        }
        
        for (int l = 0; l<this.Layers.Count - 1; l++)
            this.Layers[l].Display(neuralNet.Layers[l]);

        this.Layers[Layers.Count - 1].Display(neuralNet.Layers[neuralNet.Layers.Length - 1].OutputCount);

        StartCoroutine(DrawConnections(neuralNet));
    }

    private IEnumerator DrawConnections(NeuralNetwork neuralNet)
    {
        yield return new WaitForEndOfFrame();

        for (int l = 0; l < this.Layers.Count - 1; l++)
            this.Layers[l].DisplayConnections(neuralNet.Layers[l], this.Layers[l + 1]);
        
        this.Layers[this.Layers.Count - 1].HideAllConnections();

    }
    #endregion
}
