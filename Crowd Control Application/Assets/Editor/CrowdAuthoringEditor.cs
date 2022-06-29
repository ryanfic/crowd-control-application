using UnityEngine;
using UnityEditor;

/*
    A custom inspector for the CrowdAuthoring script, which makes it so
    the inspector only shows the relevant fields for the actions chosen for the
    crowd agent
*/
[CustomEditor(typeof(CrowdAuthoring))]
public class CrowdAuthoringEditor : Editor
{
    public override void OnInspectorGUI(){ 
        CrowdAuthoring author = (CrowdAuthoring) target;
        SerializedObject so = new SerializedObject(target); // The Serialized Object of the script
        SerializedProperty prop; // A holder for a Serialized Property from the Serialized Object

        so.Update();
        GUILayout.BeginVertical();
        {
            author.hasGoHomeAction = GUILayout.Toggle(author.hasGoHomeAction, "Has Go Home Action"); // update the has go home action boolean
            if(author.hasGoHomeAction){ //if the agent has the go home action, show the go home action information
                GUILayout.BeginVertical();
                {
                    
                    GUILayout.BeginHorizontal(); // Show the go home priority and id in a horizontal line
                    {
                        prop = so.FindProperty("goHomePriority"); // get the go home priority of the agent
                        EditorGUILayout.PropertyField(prop); // display the go home priority
                        prop = so.FindProperty("goHomeActionID"); // get the go home id of the agent
                        EditorGUILayout.PropertyField(prop); // display the go home id
                    }
                    GUILayout.EndHorizontal();
                    prop = so.FindProperty("homePoint"); // get the go home point of the agent
                    EditorGUILayout.PropertyField(prop); // display the go home point
                }
                GUILayout.EndVertical();
            }
            author.hasGoToAndWaitAction = GUILayout.Toggle(author.hasGoToAndWaitAction, "Has Go To And Wait Action"); // update the has go to and wait action boolean
            if(author.hasGoToAndWaitAction){ //if the agent has the go to and wait action, show the go to and wait action information
                GUILayout.BeginVertical();
                {
                    
                    GUILayout.BeginHorizontal(); // Show the go to and wait priority and id in a horizontal line
                    {
                        prop = so.FindProperty("goToAndWaitPriority"); // get the go to and wait priority of the agent
                        EditorGUILayout.PropertyField(prop); // display the go to and wait priority
                        prop = so.FindProperty("goToAndWaitActionID"); // get the go to and wait id of the agent
                        EditorGUILayout.PropertyField(prop); // display the go to and wait id
                        
                    }
                    GUILayout.EndHorizontal();
                    prop = so.FindProperty("timeToWait"); // get the go to and wait time of the agent
                    EditorGUILayout.PropertyField(prop); // display the go to and wait time
                    prop = so.FindProperty("waitPoint"); // get the go to and wait point of the agent
                    EditorGUILayout.PropertyField(prop); // display the go to and wait point
                }
                GUILayout.EndVertical();
            }
            author.hasFollowWayPointsAction = GUILayout.Toggle(author.hasFollowWayPointsAction, "Has Follow Waypoints Action"); // update the has follow waypoints action boolean
            if(author.hasFollowWayPointsAction){ //if the agent has the follow waypoints action, show the follow waypoints action information
                GUILayout.BeginVertical();
                {
                    
                    GUILayout.BeginHorizontal(); // Show the follow waypoints priority and id in a horizontal line
                    {
                        prop = so.FindProperty("followWayPointsPriority"); // get the follow waypoints priority of the agent
                        EditorGUILayout.PropertyField(prop); // display the follow waypoints priority
                        prop = so.FindProperty("followWayPointsID"); // get the follow waypoints id of the agent
                        EditorGUILayout.PropertyField(prop); // display the follow waypoints id
                        
                    }
                    GUILayout.EndHorizontal();
                    prop = so.FindProperty("wayPoints"); // get the waypoints of the agent
                    EditorGUILayout.PropertyField(prop); // display the waypoints
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndVertical();
        so.ApplyModifiedProperties();
    }
}
