# Heimat Hamm
Android and iOS application to allow users to discover facts about Hamm using augmented reality.


# Wezit Entity:
* https://studio.wezit.io/
* "entityId": "005196"
* "appId": "wzobj:application_27a92716-cc16-46c6-b4fe-5fa4d5bc5190"

## About
* Unity app built on iOS and Android
* Unity version: 6000.3.9f1

## App structure
The app is structured around several views managed by a View Manager, each of them
containing a control script and several sub-components.
When a user navigates through the app, the suiting view will be enabled by the Manager.

### App initialization
When the app starts, it loads data in the following order:
* app config file (config.json) containing a link to the Wezit app manifest (which itself
contains all needed link to access Wezit data)
* Wezit data:
	* Download the manifest.json file if it is missing or needs an update (which is
checked using its remote and local ETag)
	* Download/update the content database file (sqlite.sqlite), settings file
(settings.json), and assets file (assets.json) that respectively contain
		* The content tree and text content
		* The global app settings, e.g. app colour, home text, button text, …
		* Information on the assets used in the app, e.g. image uri, activity uri, …
	* Setup the different stores (see below) for easier data access later on
	* Download/update the assets used in the app, i.e. the images, videos, audios,
3D models, and activities
	* Initialize the player data
* Create a player.dat file if it does not exist
* Load the save data to save CPU usage by not having to load the text file
each time
* Initialize the global settings for easier global access, e.g. app colour, map pin
sprites, challenge scores, …
*Start the first view (i.e. either language selection or home)

### Stores
There are five stores that are used in the app:
* The tour store, granting quick access to every tour (guides in this context)
* The POI store, granting quick access to every POI (both tour POI and content)
* The POI location store, granting quick access to the geolocation of the POIs
* The cover store, granting access to video and music image covers

On top of that, the StoreAccessor references the current state of the app, which is composed
of the following elements:
* Current KioskState, i.e. current view
* Current language used by the app
* Access to the afore-mentioned stores
* Current selected POI
* Current selected tour
* Current selected image bank of the tour

# Content
All content is managed by the Wezit CMS.

The POI map is managed using the [OnlineMap](https://infinity-code.com/assets/online-maps) plugin and instantiating markers at the position written in the Wezit CMS data.

Using the tour map, users go to their first chosen POI and look for the corresponding AR
marker. By scanning the AR marker, they start the first AR session.
The image recognition is achieved with Unity AR Foundation’s AR tracked image manager.
When initializing the view content, we create a new runtime reference library containing the
POI reference image set in the Wezit CMS. AR Foundation will then look for this image and
inform the script when it is detected, along with its position. It will then use real-world reference points to maintain objects in place.


## Wezit data structure (except for the settings)

- Application
  - Tour 1 (e.g. Toni) → correct tag (e.g. "toni")
    - POI 1 (e.g. Station)
      - POI's location → tag "station_location"
        - Image used for the marker (as a RefImage)
        - GPS location of POI 1
        - Adress of POI 1
        - Thumbnail (as an image/ShowPicture)
        - "spatial" field: marker's rotation in relation to the North (needed to orientate objects based on their GPS location)
        - "durée/extent" field: vertical offset to add to the AR scene, in centimeters (e.g. -20 if all objects are 20cm too high"
      - Avatar's location → tag "avatar"
        - GPS location of the avatar
        - "spatial" field: rotation of the avatar in relation to the North
        - "durée/extent" field: scale applied to the avatar (e.g. 2 to multiply the avatar's size by 2)
      - HIdden object → tag "hidden_object"
        - .glb of the hidden object
        - GPS location of the hidden object
        - "spatial" field: rotation of the object in relation to the North
        - "durée/extent" field: scale applied to the object (e.g. 0.5 to reduce the object's size in half)
      - Portal position → tag "portal"
        - GPS location of the portal
        - "spatial" field: rotation of the portal in relation to the North
        - "durée/extent" field: scale applied to the portal
      - Past objects → tag "past_objects"
        - .mp4 of the past character's video with blue background for chroma keying
        - .mp3 of the past character's speech
        - .srt of the subtitles of the past character's speech
        - GPS location of the past character (i.e. the video)
        - Past object 1: example of a 3D object
			- .glb of the object
			- GPS location of the object
			- "spatial" field: rotation of the object in relation to the North
			- "durée/extent" field: scale applied to the object
		- Past object 2: example of a small panel
			- Main image as a showPicture
			- Back poster image(s) as refPicture(s)
			- GPS location of the panel
			- "spatial" field: rotation of the panel in relation to the North
			- "durée/extent" field: scale applied to the panel
		- ...
	  - POI carrousel → tag "carrousel"
		- Images displayed in the carrousel
	  - POI minigame → tag "minigame"
		- The POI type field selects the type of minigame, see the [minigame section](#_Gestion_des_mini-jeux)
	- POI 2
	  - ...
  - Tour 2 (e.g. Grete) → correct tag (e.g. "grete")
	  - ...
  - ...

## Augmented reality scene

The augmented reality scenes are divided into five distinct steps:

1. Looking for the marker
2. Listening to the avatar's speech
3. Looking for the hidden object
4. Crossing the portal
5. Listening to the past character's speech while walking amongst the objets

### Looking for the marker

The marker can be managed via the POI tagged 'station_location', more specifically through its thumbnail/refImage.

This same POI contains the information needed to place the entire AR scene via two fields: its placement on an online map, which gives the GPS position of the centre of the scene, and the 'spatial' field, which gives the orientation of the marker in the real world.

Indeed, objects in the AR scene are placed according to their own GPS position, so a reference point is needed to place them correctly.

### Listening to the avatar's speech

As the text spoken by the avatar is specific to the tour/avatar, the audio file and subtitle file must be added to the station's POI.

The avatar, positioned, rotated, and scaled via the POI tagged 'avatar', will use its speech animation automatically for as long as the audio subtitles contain text, pausing when there is a pause in the audio.

It is also possible to trigger avatar-specific animations using the subtitles, as explained in [the documentation on this subject](https://mazedia.sharepoint.com/:w:/s/AO_HAMM_1411-Avatare3D/EVisqpybJ1RNs-PzJVal06YBK81lOur7Il1XmSp71bEbOA?e=4gKK2D).

Once the audio has finished, the hidden object search phase starts.

### Looking for the hidden object

The hidden object is a .glb file added to the POI tagged 'hidden_object', whose placement, orientation, and scale can be managed within that same POI.

The object appears automatically once the avatar's text is finished. When the user spots it and touches it (by tapping the screen), the portal appears.

### Crossing the portal

The portal is positioned, rotated, and scaled using the POI tagged 'portal'.

As long as it has not been crossed, the user can see the past scene through it, which is otherwise invisible. Once crossed, the 'sepia' effect is activated, and objects from the past become fully visible.

### Past objects

There are three types of objects from the past: video (.webm), 3D objects (.glb), and panels with images.

#### Videos
The video, audio, and subtitles corresponding of the past character are added to the POI tagged 'past_objects'. 

The video is placed, rotated, and scaled via this same POI. When the audio is finished, the user unlocks a new seed and has the choice between continuing their AR experience or discovering the POI file.

#### Past objects

To add a 3D object, you must first create an empty POI, position it on an online map, then indicate its rotation relative to the marker in the 'spatial/räumlich' field (it is aligned with the marker by default).

It is also possible to change its scale via the 'duration/extent' field, its size will then be multiplied by the value entered.

To add a 3D object, simply create an empty POI and add a 'has 3D model' media type with a .glb file.

#### Images

As with 3D objects, you must first create an empty POI, position it on an online map, then indicate its rotation relative to the marker in the 'spatial/räumlich' field (it is aligned with the marker by default).

It is also possible to change its scale via the 'duration/extent' field, its size will then be multiplied by the value entered.

There are three subtypes of panels with images, which can be selected via the POI's 'type' field:

- 'small': 1.8 m high panel
- 'medium': 3.5 m high panel
- 'large': 5 m high panel

The image displayed on the panel must be entered in the POI media, under the type 'image/showPicture'. The images displayed on the back of the panel (the "posters") must be entered under the type 'thumbnail/refPicture'. Each addition will activate a 'poster' location on the structure.

As each type of panel is a different size, they cannot accommodate the same number of 'posters'. When the maximum number for the chosen panel type is reached, the following "poster" images are ignored. If no 'posters' are added, no slots are activated on the structure.

The limitations are as follows:

- 'small': 1 poster
- 'medium': 4 posters
- 'large': 5 posters

## Minigame management

As each station's minigame is independent of the selected tour/guide, it is managed by a POI that is a child of the guide's own POI (see above) with the tag 'minigame'.

The type of minigame is indicated by the POI's _type_ field, according to the keyword indicated below. The content is entered depending on each type of minigame.

The title and body text of the minigame tutorial pop-in must be entered in the POI itself, in the _subject_ (for the title) and _description_ (for the body text) fields.

### Siding puzzle minigame

_Type_ field keyword: "sliding_puzzle"

The mini-game data must be entered in a "taquin" activity:

- Image: selected from the 'activity' template
- Number of cells: selected from the 'activity' template

### Negotiation minigame (quiz)

Type field keyword: "quiz"

The minigame data must be entered in a "quiz" activity:

- Questions: entered in the 'activity' template
- Answers: entered in the 'activity' template

### Diaporama minigame

_Type_ field keyword: "diaporama"

The minigame data must be entered directly in the POI:

- Background image: image of type _vignette/refImage_
- Superimposed images: images of type _image/showPicture_

### Touch minigame (Selection)

_Type_ field keyword: "touch"

The minigame data must be entered in a "selection" activity:

- Background image: entered in the 'activity' template
- Touch zones: entered in the 'activity' template

### ARTouch minigame

_Type_ field keyword: "ar"

The minigame data must be entered directly in the POI:

- Number of objects to touch: entered in the 'duration/extent' field.
- Period during which objects appear: entered in the 'location' field.
- Percentage chance of a negative object appearing: entered in the 'spatial' field. Please note that leaving this field blank or setting it to 0 will disable the appearance of negative objects and therefore the concept of limited lives.
- Spawn radius of the objects: set in the "source" metadata field
- Lifetime of the objects: set in the "author" metadata field

The "ARTouch" minigame has animated tool and items, whose sprites and sound effects can be managed as follow:

- The "main tool" of the game is managed in the minigame POI itself
  - Every showPicture relation image is a frame in the animation
  - The audioClip relation is the sound effect played when the user taps their screen
- The "good item" that can spawn (e.g. flames or soldiers) are managed in a child POI tagged "ARItemGood"
  - Every showPicture relation image is a frame in the animation
  - The audioClip relation is the sound effect played when the user hits the item
- The "bad item" that can spawn (e.g. Klippi) are managed in a child POI tagged "ARItemBad"
  - Every showPicture relation image is a frame in the animation
  - The audioClip relation is the sound effect played when the user hits the item

### Music AR minigame

Keyword in the metadata field _type_: "music"

The minigame data are entered directly in the POI:

- Number of items to touch: set in the "extent" metadata field
- Spawn rate of the objects: set in the "localization" metadata field
- Spawn radius of the objects: set in the "source" metadata field
- Lifetime of the objects: set in the "author" metadata field
- Scale of the objects: set in the "spatial" metadata field

The "ARTouch" minigame has several music notes that the user must touch to complete the game. When the user touches a note, a sound is played, and the note is added to the collected top bar. Once the user has collected six notes, the game ends and a melody is played. The notes data must be entered as follow:

- Each note is a child POI with:
  - Alternative sprites as show picture relation
  - "Collected" sprite as ref picture relation
  - Touch sound effect as play track relation
  - End melody as ambient sound relation
- The default music (when the user has collected more than one type of note) as the minigame POI ambient sound relation

## Organisation and types of settings

The settings are organised by screen as much as possible, with the following divisions:

- accessibility: all settings linked to the app's global accessibility
- home: the tour choice screen settings, including the tutorial
  - home: the texts of the tour choice screeen
  - tutorial: the texts and image of the tutorial
- language: the settings of the language selection screen, and the menu's language buttons
- splash: the splash screen video. It will be downloaded if needed when the tour selection screen is opened, allowing the client to update it.
- tour.intro : the selected tour screen settings

The settings are named using the following structure:

contexte_large.contexte_précis.objet_du_setting.type_de_setting

big_context.smaller_context.the_thing.type_of_the_thing

The setting modifying the tour start button can be decomposed as follow:

tour.intro.start.button.text

And the splash screen video setting this way:

splash.screen.background.video