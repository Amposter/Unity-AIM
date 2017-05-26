# UnityAIM

Autonomous vehicles are a popular area of study in machine learning that has seen great interest due to the increased funding by prominent companies. This purpose of this Unity3D project is to investigate ways to automatically produce decentralised car control-systems for future autonomous vehicle applications by using machine learning. 

UnityAIM is a simulator built to test the efficacy of using a deterministic approach called Autonomous Intersection Management (AIM) for coordinated driving against two decetralised approaches that uses neuro-evolution (NEAT and HyperNEAT). Each approach is evaluated by its ability to coordinate a group of vehicles through 10 different <a href="https://github.com/Amposter/Unity-AIM/blob/master/Appendices/Tracks/Tracks.md" target="_blank">tracks</a> that were modelled off real road networks.

![Failed to load - see appendices](https://github.com/Amposter/Unity-AIM/blob/master/Appendices/background1.png "UnityAIM in action")

The project is still very much work-in-progress. Examples, videos and detailed download instructions will be added at the soonest!

## Instructions

The project is compatible with any version of Windows that can run Unity 5 or later releases. There are a few minor issues for Mac which are currently fixed. The scenes for both experiments are similar however each implementation is done a separate branch. Details are provided below for each of the experiments.

### AIM

The implementation of AIM is included on the 'master' branch. Once the project is opened in Unity, open the scene: "Assets/Imports/Demo Tracks And Controllers/track_composer.unity". The parent object of all the tracks is located in the Hierarchy as 'Tracks'. Once the 'Tracks' object is expanded, enable only the track you want to test. To do this, select the track and ensure that the checkbox next to its name in the Inspector is selected. Disable all other tracks by deselecting this checkbox. In order to change the track parameters, expand the track object and select the 'CarSpawner' object. In the Inspector, you'll find the variables that can be changed under the 'Car Spawner' script. The only variables you should edit for testing is the 'Test Duration' which is the length of each trials in seconds and the 'Cars Per Start Point' which specifies the maximum of cars that can spawn at each entry point. Once you're happy with your setup, hit play to watch AIM in action. You may need to adjust the camera as certain tracks will be out of view. It is better to to switch to scene once playing to zoom in/out or to view different parts of the bigger tracks.

### NEAT
