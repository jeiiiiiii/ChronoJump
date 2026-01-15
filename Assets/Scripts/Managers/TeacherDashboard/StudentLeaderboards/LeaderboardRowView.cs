using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRowView : MonoBehaviour
{
    [Header("UI References")]
    public Image LeaderboardStudIcon;
    public TextMeshProUGUI LeaderboardStudName;
    public TextMeshProUGUI LeaderboardStudPoints;
    public TextMeshProUGUI rankingText;
    public Image leaderboardStudPointsBg;
    
    public Sprite firstTotalPointsBackground;
    public Sprite secondTotalPointsBackground;
    public Sprite thirdTotalPointsBackground;
    public Sprite followingTotalPointsBackground;
    
    public void SetupStudent(LeaderboardStudentModel student, int ranking)
    {
        if (LeaderboardStudName != null)
            LeaderboardStudName.text = student.displayName ?? "Unknown Student";
        
        if (LeaderboardStudPoints != null)
            LeaderboardStudPoints.text = student.overallScore.ToString();
        
        if (rankingText != null)
            rankingText.text = ranking.ToString();
        
        ApplyRankingStyling(ranking);
    }

    /// <summary>
    /// Show placeholder values when no real data exists.
    /// </summary>
    public void SetDefaultStudent(int ranking)
    {
        if (LeaderboardStudName != null)
            LeaderboardStudName.text = "No Student";

        if (LeaderboardStudPoints != null)
            LeaderboardStudPoints.text = "0";

        if (rankingText != null)
            rankingText.text = ranking.ToString();

        ApplyRankingStyling(ranking);
    }
    
    private void ApplyRankingStyling(int ranking)
    {
        switch (ranking)
        {   
            case 1:
                SetRankingAssets(firstTotalPointsBackground, 
                    Hex("#FACC15"), Hex("#FEF9C3"), Hex("#A16207"));
                break;
            case 2:
                SetRankingAssets(secondTotalPointsBackground, 
                    Hex("#9CA3AF"), Hex("#FFFFFF"), Hex("#374151"));
                break;
            case 3:
                SetRankingAssets(thirdTotalPointsBackground, 
                    Hex("#FB923C"), Hex("#FFFFFF"), Hex("#C2410C"));
                break;
            default:
                SetRankingAssets(followingTotalPointsBackground, 
                    Hex("#60A5FA"), Hex("#FFFFFF"), Hex("#1D4ED8"));
                break;
        }
    }
    
    private void SetRankingAssets(Sprite bgSprite, Color rankColor, Color iconColor, Color pointsColor)
    {
        if (leaderboardStudPointsBg != null && bgSprite != null)
        {
            leaderboardStudPointsBg.sprite = bgSprite;
            leaderboardStudPointsBg.color = iconColor;
        }
        
        if (rankingText != null)
            rankingText.color = rankColor;

        if (LeaderboardStudPoints != null)
            LeaderboardStudPoints.color = pointsColor;
    }

    private Color Hex(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;
    }
}
