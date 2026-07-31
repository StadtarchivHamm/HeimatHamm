# Heimat Hamm
## Keystore
HeimatHamm

## Plugins / Extensions 

* Unity UI extensions : installer le package par nom _com.unity.uiextensions_
* Unidux / UniRx / MiniJSON : Suivre la procédure officielle (https://github.com/mattak/Unidux)
* glb/gltf : installer les packages suivants par nom 
	* _com.unity.cloud.gltfast_
	* _com.unity.cloud.draco_
	* _com.unity.cloud.ktx_
* Matomo : télécharger/importer le package (https://github.com/lumpn/unity-matomo) + paramétrer configuration de "analyticsMatomoSettings" dans config.json (cf. https://matomo1.wezit.io/)

# Wezit

## Entité Wezit : (exemple Wezit V3)
Les paramètres Wezit sont à saisir dans le fichier wezit_config.json. Au début du projet, modifier le lien vers le manifest pour celui de votre application.

Exemple de lien : https://static.wezit.io/099998/appdata/wzobj_application_cb4ba3ba-834e-4834-b5dd-9557cffac757/manifest.json

## Data Grabber (exemple Wezit V3)
Le téléchargement des données (fichiers de données et assets) se fait automatiquement au démarrage de l'application.

## A propos

* Application Unity compilée en Standalone Windows (par défaut)
* Version Unity : 6000.0.23f1

## Build

Pour compiler en ligne de commande :
`[Path de Unity Editor] -projectPath [Path du projet Unity] -batchmode -buildTarget Standalone -executeMethod AutomaticBuild.Perform [Path de la scene à intégrer à la build] -quit`
`[Path de Unity Editor] -projectPath [Path du projet Unity] -batchmode -buildTarget WebGL -executeMethod AutomaticBuild.Perform [Path de la scene à intégrer à la build] -quit`

Path de la scene à intégrer à la build :
`Assets/Scenes/Main.unity`