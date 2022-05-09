using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Code Breaker Level", menuName = "New Code Breaker Level", order = 56)]
public class CodeBreakerLevel : LevelBase
{
    //Objective 1 - Completed
    //Objective 2 - Number of guesses 1
    //Objective 3 - Number of guesses 2

    public List<CodeBreakerChoicesInfo.CodebreakerChoices>  solution;
    public List<CodeBreakerChoicesInfo.CodebreakerChoices>  possibleChoices;

    public int                                              highNumberOfGuessesGoal;
    public int                                              lowNumberOfGuessesGoal;
}
