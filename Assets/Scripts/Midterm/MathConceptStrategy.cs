using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Implementation of IExplanationStrategy for mathematical concepts
public class MathConceptStrategy : MonoBehaviour, IExplanationStrategy
{
    [Header("Math Explanation UI")]
    [SerializeField] private GameObject mathPanel;
    [SerializeField] private TMP_Text mathText;
    [SerializeField] private Button showMathButton;
    [SerializeField] private Button closeMathButton;

    private void Start()
    {
        SetupButtons();
        InitializeUI();
    }

    private void SetupButtons()
    {
        if (showMathButton != null)
        {
            showMathButton.onClick.AddListener(() => ShowKonigsbergExplanation());
        }

        if (closeMathButton != null)
        {
            closeMathButton.onClick.AddListener(() => HideExplanation());
        }
    }

    private void InitializeUI()
    {
        if (mathPanel != null)
        {
            mathPanel.SetActive(false);
        }
    }

    #region IExplanationStrategy Implementation
    public void ShowExplanation(string content)
    {
        if (mathPanel != null)
        {
            mathPanel.SetActive(true);
        }

        if (mathText != null)
        {
            mathText.text = FormatMathematicalContent(content);
        }

        Debug.Log("Showing mathematical concept explanation");
    }

    public void HideExplanation()
    {
        if (mathPanel != null)
        {
            mathPanel.SetActive(false);
        }

        Debug.Log("Hiding mathematical concept explanation");
    }
    #endregion

    #region Public Methods
    public void ShowKonigsbergExplanation()
    {
        string konigsbergExplanation =
            "The Seven Bridges of Königsberg is a famous mathematical problem solved by Euler in 1736.\n\n" +
            "The Challenge: Cross each bridge exactly once.\n\n" +
            "Euler's Discovery: This is mathematically impossible!\n\n" +
            "Why? For an Eulerian path to exist, at most two vertices can have an odd number of edges. " +
            "In Königsberg, all four land masses have an odd number of bridges.";

        ShowExplanation(konigsbergExplanation);
    }
    #endregion

    #region Helper Methods
    private string FormatMathematicalContent(string content)
    {
        // Add mathematical formatting and structure
        string formattedContent = $"<b>Mathematical Concept</b>\n\n{content}";

        // Add Königsberg-specific information if needed
        if (content.ToLower().Contains("bridge") || content.ToLower().Contains("euler"))
        {
            formattedContent += "\n\n<i>Historical Note: This problem was solved by Leonhard Euler in 1736, founding the field of graph theory.</i>";
        }

        return formattedContent;
    }
    #endregion

    #region Context Menu Testing
    [ContextMenu("Test Math Explanation")]
    private void TestMathExplanation()
    {
        if (Application.isPlaying)
        {
            ShowKonigsbergExplanation();
        }
    }
    #endregion
}