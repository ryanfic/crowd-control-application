/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using Unity.Entities;

[GenerateAuthoringComponent]
public struct GameState : IComponentData {

    public enum State {
        WaitingToStart,
        Playing,
        Dead
    }

    public State state;

}