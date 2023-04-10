#region Includes
using UnityEngine;
using System.Collections.Generic;
#endregion

public class UINeuralNetworkLayerPanel : MonoBehaviour
{
    #region Members
    [SerializeField]
    private RectTransform LayerContents;
    [SerializeField]
    public List<UINeuralNetworkConnectionPanel> Nodes;
    #endregion

    #region Methods
    public void Display(NeuralLayer layer)
    {
        Display(layer.NeuronCount);
    }
    public void Display(uint neuronCount)
    {
        UINeuralNetworkConnectionPanel dummyNode = Nodes[0];

        for (int i = Nodes.Count; i < neuronCount; i++)
        {
            UINeuralNetworkConnectionPanel newNode = Instantiate(dummyNode);
            newNode.transform.SetParent(LayerContents.transform, false);
            Nodes.Add(newNode);
        }

        for (int i = this.Nodes.Count - 1; i >= neuronCount; i++)
        {
            UINeuralNetworkConnectionPanel toBeDestroyed = Nodes[i];
            Nodes.RemoveAt(i);
            Destroy(toBeDestroyed);
        }
    }

    public void DisplayConnections(NeuralLayer currentLayer, UINeuralNetworkLayerPanel nextLayer)
    {
        for (int i = 0; i<Nodes.Count; i++)
            Nodes[i].DisplayConnections(i, currentLayer, nextLayer);
    }

    public void HideAllConnections()
    {
        foreach (UINeuralNetworkConnectionPanel node in Nodes)
            node.HideConnections();
    }
    #endregion
}
