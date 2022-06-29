using UnityEngine;
using UnityEditor;

/*
    A custom inspector for the ReynoldsBehaviourAuthoring script, which makes it so
    the inspector only shows the relevant fields for the behaviours chosen for the
    game object
*/
[CustomEditor(typeof(ReynoldsBehaviourAuthoring))]
public class ReynoldsBehaviourEditor : Editor
{
    public override void OnInspectorGUI(){ 
        //base.OnInspectorGUI();

        ReynoldsBehaviourAuthoring author = (ReynoldsBehaviourAuthoring) target;
        SerializedObject so = new SerializedObject(target); // The Serialized Object of the script
        SerializedProperty prop; // A holder for a Serialized Property from the Serialized Object

        so.Update();
        GUILayout.BeginVertical();
        {
            prop = so.FindProperty("maxVelocity"); // get the max velocity of the agent
            EditorGUILayout.PropertyField(prop); // display the max velocity
            author.flocking = GUILayout.Toggle(author.flocking, "Flocking"); // update the flocking boolean
            if(author.flocking){ //if the agent has the flocking behaviour, show the flocking information
                GUILayout.BeginVertical();
                {
                    prop = so.FindProperty("flockingWeight"); // get the flocking weight of the agent
                    EditorGUILayout.PropertyField(prop); // display the flocking weight
                    GUILayout.BeginHorizontal(); // Show the Avoidance Radius and Weight in a horizontal line
                    {
                        prop = so.FindProperty("avoidanceRadius");
                        EditorGUILayout.PropertyField(prop);
                        prop = so.FindProperty("avoidanceWeight");
                        EditorGUILayout.PropertyField(prop);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(); // Show the Cohesion Radius and Weight in a horizontal line
                    {
                        prop = so.FindProperty("cohesionRadius");
                        EditorGUILayout.PropertyField(prop);
                        prop = so.FindProperty("cohesionWeight");
                        EditorGUILayout.PropertyField(prop);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            author.fleeing = GUILayout.Toggle(author.fleeing, "Fleeing"); // update the fleeing boolean
            if(author.fleeing){ //if the agent has the fleeing behaviour, show the fleeing information
                GUILayout.BeginVertical();
                {
                    prop = so.FindProperty("fleeingWeight"); // get the fleeing weight of the agent
                    EditorGUILayout.PropertyField(prop); // display the fleeing weight
                    prop = so.FindProperty("fleeSafeDistance"); // get the safe distance for fleeing of the agent
                    EditorGUILayout.PropertyField(prop); // display the fleeing safe distance
                    prop = so.FindProperty("fleeTargetPos"); // get the flee target position of the agent
                    EditorGUILayout.PropertyField(prop); // display the flee target position
                }
                GUILayout.EndVertical();
            }
            author.seeking = GUILayout.Toggle(author.seeking, "Seeking"); // update the seeking boolean
            if(author.seeking){ //if the agent has the seeking behaviour, show the seeking information
                GUILayout.BeginVertical();
                {
                    prop = so.FindProperty("seekingWeight"); // get the seeking weight of the agent
                    EditorGUILayout.PropertyField(prop); // display the seeking weight
                    prop = so.FindProperty("seekTargetPos"); // get the seek target position of the agent
                    EditorGUILayout.PropertyField(prop); // display the seek target position
                }
                GUILayout.EndVertical();
            }
            author.avoidingObstacles = GUILayout.Toggle(author.avoidingObstacles, "Avoiding Obstacles"); // update the avoiding obstacles boolean
            if (author.avoidingObstacles)
            { //if the agent has the obstacle avoidance behaviour, show the obstacle avoidance information
                GUILayout.BeginVertical();
                {
                    prop = so.FindProperty("obstacleAvoidanceWeight"); // get the obstacle avoidance weight of the agent
                    EditorGUILayout.PropertyField(prop); // display the obstacle avoidance weight
                    prop = so.FindProperty("movementPerRay"); // get the movement per ray of the agent
                    EditorGUILayout.PropertyField(prop); // display the movement per ray of the agent
                    prop = so.FindProperty("numberOfRays"); // get the number of rays of the agent
                    EditorGUILayout.PropertyField(prop); // display the number of rays of the agent
                    prop = so.FindProperty("visionAngle"); // get the vision angle of the agent
                    EditorGUILayout.PropertyField(prop);
                    prop = so.FindProperty("visionLength"); // get the vision length of the agent
                    EditorGUILayout.PropertyField(prop);
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndVertical();
        so.ApplyModifiedProperties();






        /*SerializedProperty prop = so.FindProperty("floats");
        SerializedProperty prop2 = so.FindProperty("leeks");
        SerializedProperty temp1;
        SerializedProperty temp2;
        string prop1name = "";
        string prop2name = "";
        GUILayout.BeginVertical();
            for(int i = 0; i < prop.arraySize; i++){
                prop1name = "floats.Array.data["+i+"]";
                prop2name = "leeks.Array.data["+i+"]";
                temp1 = so.FindProperty(prop1name);
                temp2 = so.FindProperty(prop2name);

                GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(temp1);
                    EditorGUILayout.PropertyField(temp2);
                GUILayout.EndHorizontal();
            }
        GUILayout.EndVertical();
        //so.Update();
        GUILayout.Label("Neato Label", EditorStyles.boldLabel);
        GUILayout.BeginVertical();
            author.flocking = GUILayout.Toggle(author.flocking, "Flocking");
            
            GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prop);
                EditorGUILayout.PropertyField(prop2);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                if(GUILayout.Button("Generate Color")){
                    Debug.Log("Size of prop:" + prop.arraySize);
                }
                if(GUILayout.Button("Reset")){
                    Debug.Log("Reset!");
                }
            GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        so.ApplyModifiedProperties();*/
    }
}
