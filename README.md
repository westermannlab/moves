# The MOVES paradigm: a tutorial

<h2>1. Introduction</h2>
<p>The MOVES paradigm contains the three configuration files <i>moves.ini, tutorial.ini</i> and <i>personalities.ini</i>, which can be used to modify the program and translate it into other languages. The contents of the files are formatted using the JSON format. If they cannot be read due to a syntax error or cannot be found, an error message is displayed in the main menu of MOVES.</p>
<p>The files can be found in the “StreamingAssets” folder, which is located in the “moves_Data” folder in Windows builds. The configuration options for the three files are described below.</p>
<p>In the following, the contents of those files are described in detail. In addition, the log files that are created for each encounter are explained and the artificial motivation used is illustrated by means of decision tables.</p>

<h2>2. MOVES.INI (Configuration file)</h2>
<p>The configuration file moves.ini contains some general settings regarding gameplay, encounter sequence and the evaluation at the end of the encounters. The possible settings are described below.</p>

<h3>2.1 “version”</h3>
<p>The version of the <i>moves.ini</i> file. Is intended to facilitate the management of various configurations. Shows up in the log files (see 5. Log files).</p>
<h3>2.2 “vp”</h3>
<p>The participant’s unique identifier to allow easier allocation of the log files later. Can contain letters, numbers and characters.</p>
<p>Example:</p>
<p><code>“vp”: “1”</code></p>

<h3>2.3 “roomOrder” </h3>
<p>The order in which the encounters are unlocked. The numbers from 1 to 5 represent the five levels and can be arranged in any order.</p>
<p>Example:</p>
<p><code>“roomOrder”: [ 2, 1, 3, 4, 5 ]</code></p>
<p><i>The player will meet the computer-controlled player with personality no. 2 during the first encounter, followed by player no. 1 during the second encounter, followed by players no. 3, 4 and 5.</i></p>

<h3>2.4 “roomVisits”</h3>
<p>The number of completions of all five encounters and the tutorial. The first value represents the tutorial, the other values correspond to the five encounters. Here, the second value denotes the encounter that is first in the order defined above, the third value denotes the encounter that is second, and so on. If MOVES is operated in default mode (debug=0, see 2.7 “debug”), each encounter can be entered only once.</p>
<p>Example:</p>
<p><code>“roomVisits”: [ 1, 1, 0, 0, 0, 0]</code></p>
<p><i>The player has completed the tutorial and the first encounter. When the game starts with this setting, the second encounter will be accessible right away.</i></p>

<h3>2.5 “roomCaptions”</h3>
<p>The captions on the buttons in the main menu.</p>
<p>Example:</p>
<p><code>“roomCaptions”: [ “Tutorial”, “Encounter 1”, “2”, “3”, “4”, “5” ]</code></p>
<p><i>The button that starts the tutorial is labeled “Tutorial”. The button that starts the first encounter is labeled “Encounter 1”. The remaining four buttons are labeled “2”, “3”, “4”, and “5”.</i></p>

<h3>2.6 “duration”</h3>
<p>The duration of each encounter in seconds. The tutorial is not affected by this setting.</p>
<p>Example:</p>
<p><code>“duration”: 240.0</code></p>
<p><i>The encounters last 240 seconds.</i></p>

<h3>2.7 “debug”</h3>
<p>If the debug value is set to “2” here, MOVES starts in windowed mode and all encounters are accessible at any time. During the encounters, the enter key can be used to open a console that allows various commands to be entered. Entering “help” shows an overview of the most important commands.</p>
<p>Example:</p>
<p><code>“debug”: 0</code></p>
<p><i>MOVES starts in default mode.</i></p>

<h3>2.8 Various messages, labels and placeholders</h3>
<p>><strong>“msgRoomAlreadyCompleted”, “msgRoomNotYetAvailable”, …, “colorBlue”, “colorPurple”</strong></p>
<p>A set of variables that can be used to change various messages, button labels and placeholders.</p>

<h3>2.9 “scales”</h3>
<p>An array of Likert scales to be filled in after each encounter during evaluation. Each scale consists of a name, an (optional) instruction, and an array of items.</p>

<h4>2.9.1 “name”</h4>
<p>The name of the Likert scale. Displayed above the scale.</p>

<h4>2.9.2 “instructions”</h4>
<p>Short, optional instructions for filling in the scale. May contain the placeholders <i>“[CAPTION]”</i> and <i>“[COLOR]”</i>, which can be defined individually for each computer-controlled player in the personalities.ini configuration file (see chapter 4).</p>
<p>Example:</p>
<p><code>“You have just met the [COLOR] player [CAPTION].”</code></p>
<p><i>…may translate into the instruction…</i></p>
<p><code>“You have just met the blue player Bob.”</code></p>

<h4>2.9.3 “items”</h4>
<p>An array of Likert scale items. Each item consists of one question and seven options.</p>

<h4>2.9.4 “question”</h4>
<p>The question or statement to which the choice options refer. Again, the placeholders <i>“[CAPTION]”</i> and <i>“[COLOR]”</i> can be used.</p>

<h4>2.9.5 “options”</h4>
<p>An array of seven labels to be displayed below the seven levels of the Likert item. An empty label (“”) leaves the according level unlabeled.</p>
<p>Example:</p>
<p><pre>“scales”: [
   {
      “name”: “Self-report 1”,
      “instructions”: “This is about the [COLOR] player you just met.”,
      “items”: [
         {   
            “question”: “Did you enjoy spending time with [CAPTION]?”,
            “options”: [
               “not at all”,
               “”,
               “”,
               “”,
               “”,
               “”,
               “totally”
            ]
         },
         {
            “question”: “Question 2”,
            “options”: [
               “left”,
               “”,
               “”,
               “center”,
               “”,
               “”,
               “right”
            ]
         }
      ]
   }
]</pre></p>
<p><i>Displays a Likert scale named “Self-report 1”. The placeholder “[COLOR]” in the instructions will be replaced with the color of the computer-controlled player. The scale contains two items, from which the first is titled “Did you enjoy spending time with [CAPTION]?”. The placeholder “[CAPTION]” is replaced with the corresponding value from the personalities.ini configuration file (see chapter 4). The first item’s left-most option is labeled “not at all”, the right-most option is labeled “totally”.</i></p>

<h2>3. TUTORIAL.INI (Configuration file)</h2>
<p>The configuration file tutorial.ini contains the contents of all text boxes along the tasks to be performed by the participants during the tutorial. By editing this file, it is possible to customize the flow of the tutorial, add your own texts, and make translations. The functionality is described below.</p>

(...)

<h2>4. PERSONALITIES.INI (Configuration file)</h2>
<p>The configuration file personalities.ini allows to customize the artificial motivation of the computer controlled players.</p>

(...)

<h2>5. Log files</h2>
<p>As soon as the participant begins an encounter with another player, all relevant actions of both characters are logged in the background until the time expires. Besides enacting and disembodying from objects, movement and collection of notes, the performed actions are logged with some situational context. When running on Windows, the log files are stored at the following path:</p>
<p><code>C:/Users/[USERNAME]/AppData/LocalLow/arne_sibilis/moves/logs/</code></p>

(...)

<h2>6. Artificial motivation tables</h2>
<p>Below is a detailed overview of all probabilities by which the artificial motivation described in chapter 4 are defined. The probability values for enacting and disembodying from objects are broken down by situation. Only objects that the player can potentially control are considered in the tables. Values highlighted in bold indicate that the corresponding change of enacted object can be performed even if the other player is currently controlling that object. Values in italics describe impossible situations and are listed only for the sake of completeness.</p>

(...)
