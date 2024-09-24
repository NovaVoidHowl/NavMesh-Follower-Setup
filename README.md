# NavMesh Follower Setup

## Script Overview

This editor script/app is to assist in setting up Nav Mesh Followers, as supported by the Mod by @kafeijao.  
The readme for the CCK plugin for this is https://github.com/kafeijao/Kafe_CVR_CCKs/tree/master/NavMeshFollower#readme  

## Path Length Issues (Unity Behaviour)
If you find that the 'App Components' section of the Tool Setup window is not showing up, then it could be that your project folder has too long a path for the legacy default Windows max path length.

In version 3.3.0, you will get a warning about this when you open the `Tool Setup` window, if the path is too long.

Unity observes this old limit, thus long paths (more than 56 chars including project name, the rest is used up in the internals of the project paths) will cause the list to not show up.

Sadly it is not possible to work around this issue as it is a Unity imposed limitation (even the Windows group policy setting `Enable Win32 long paths` has no impact on it )
 

## How to install this package

1> In unity open the `Package Manager` window

2> Click the plus button in the top left of that window and choose the `Add packages from git URL` option

![image](https://github.com/NovaVoidHowl/Mesh-Bone-Rebind/assets/31048789/66eaec96-322e-46ac-811d-353f8209198c)

3> Paste in the git url of this repo `https://github.com/NovaVoidHowl/NavMesh-Follower-Setup.git`

![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/8412f39a-0c95-4bff-84fa-e3a4d0cf65da)


4> Click the add button

The script should then be ready to use.

## How to use

When the package is installed you should see a new menu option added to the to bar of unity.  
![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/698aae76-fb41-4b6c-920b-9063e98ff686)

Click on the 'Tool Setup' option to get to this window  
![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/2c6de4a4-20f6-4632-aac6-912f2a1aadd0)

scroll down and install the 3rd party dependencys listed, once you have done that scroll to the top of the window and install the main dependancys  
![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/cfe5f73a-5fad-486c-ad7f-6e0c1c45bc34)

Then finaly install the app components
![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/6bf8e15c-277e-4d23-980c-c1f85d8a7271)

Now close and re-open unity, this is needed to ensure all packages are correctly loaded.

You are now ready to setup a Nav Mesh folower.  
To do this, create a new empty object in the Hierarchy  
![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/0273e713-f6a2-4ee8-a4f1-0dc222ba40ab)

Then add the add the Nav Mesh Follower Setup component to it  
![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/45b8c1d6-9487-4f07-bdd8-8688e2d01884)

add your follower under that object   
![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/1155d8ba-19c0-461f-86cb-47a37a961716)

now select the root object of your follower and run through the setup as guided by the component  
![image](https://github.com/NovaVoidHowl/NavMesh-Follower-Setup/assets/31048789/66b79e2d-07f3-40ff-aafa-fa2467fd0254)

