# The MOVES paradigm: a tutorial
<h4>Table of contents</h4>
<ul>
   <li><a href="#1-introduction">1. Introduction</a></li>
   <li><a href="#2-moves">2. MOVES.INI (Configuration file)</a></li>
   <li><a href="#3-tutorial">3. TUTORIAL.INI (Configuration file)</a></li>
   <li><a href="#4-personalities">4. PERSONALITIES.INI (Configuration file)</a></li>
   <li><a href="#5-log-files">5. Log files</a></li>
   <li><a href="#6-artificial-motivation">6. Artificial motivation tables</a></li>
</ul>

<h2 id="1-introduction">1. Introduction</h2>
<p>The MOVES paradigm contains the three configuration files <i>moves.ini, tutorial.ini</i> and <i>personalities.ini</i>, which can be used to modify the program and translate it into other languages. The contents of the files are formatted using the JSON format. If they cannot be read due to a syntax error or cannot be found, an error message is displayed in the main menu of MOVES.</p>
<p>The files can be found in the “StreamingAssets” folder, which is located in the “moves_Data” folder in Windows builds. The configuration options for the three files are described below.</p>
<p>In the following, the contents of those files are described in detail. In addition, the log files that are created for each encounter are explained and the artificial motivation used is illustrated by means of decision tables.</p>

<h2 id="2-moves">2. MOVES.INI (Configuration file)</h2>
<p>The configuration file <i>moves.ini</i> contains some general settings regarding gameplay, encounter sequence and the evaluation at the end of the encounters. The possible settings are described below.</p>

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
<p>The number of completions of all five encounters and the tutorial. The first value represents the tutorial, the other values correspond to the five encounters. Here, the second value denotes the encounter that is first in the order defined above, the third value denotes the encounter that is second, and so on. If MOVES is operated in default mode (debug=0, see <a href="#2-7-debug">2.7 “debug”</a>), each encounter can be entered only once.</p>
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

<h3 id="2-7-debug">2.7 “debug”</h3>
<p>If the debug value is set to “2” here, MOVES starts in windowed mode and all encounters are accessible at any time. During the encounters, the enter key can be used to open a console that allows various commands to be entered. Entering “help” shows an overview of the most important commands.</p>
<p>Example:</p>
<p><code>“debug”: 0</code></p>
<p><i>MOVES starts in default mode.</i></p>

<h3>2.8 Various messages, labels and placeholders</h3>
<p><strong>“msgRoomAlreadyCompleted”, “msgRoomNotYetAvailable”, …, “colorBlue”, “colorPurple”</strong></p>
<p>A set of variables that can be used to change various messages, button labels and placeholders.</p>

<h3>2.9 “scales”</h3>
<p>An array of Likert scales to be filled in after each encounter during evaluation. Each scale consists of a name, an (optional) instruction, and an array of items.</p>

<h4>2.9.1 “name”</h4>
<p>The name of the Likert scale. Displayed above the scale.</p>

<h4 id="2-9-2-instructions">2.9.2 “instructions”</h4>
<p>Short, optional instructions for filling in the scale. May contain the placeholders <i>“[CAPTION]”</i> and <i>“[COLOR]”</i>, which can be defined individually for each computer-controlled player in the personalities.ini configuration file (see <a href="#5-personalities">chapter 4</a>).</p>
<p>Example:</p>
<p><code>“You have just met the [COLOR] player [CAPTION].”</code></p>
<p><i>…may translate into the instruction…</i></p>
<p><code>“You have just met the blue player Bob.”</code></p>

<h4>2.9.3 “items”</h4>
<p>An array of Likert scale items. Each item consists of one question and seven options.</p>

<h4 id="2-9-4-question">2.9.4 “question”</h4>
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

<h2 id="3-tutorial">3. TUTORIAL.INI (Configuration file)</h2>
<p>The configuration file <i>tutorial.ini</i> contains the contents of all text boxes along the tasks to be performed by the participants during the tutorial. By editing this file, it is possible to customize the flow of the tutorial, add your own texts, and make translations. The functionality is described below.</p>

<h3>3.1 “version”</h3>
<p>The version of the tutorial.ini file. Is intended to facilitate the management of various configurations. Shows up in the log files (see <a href="#5-log-files">5. Log files</a>).</p>

<h3>3.2 “tasks”</h3>
<p>An array of objects, each describing a task or the contents of a text box.</p>

<h4>3.2.1 “identifier”</h4>
<p>May contain a description of the task to improve the file’s readability. Does not get processed by the MOVES program.</p>

<h4>3.2.2 “type”</h4>
<p>The type of a task. Is specified by a single-digit number. Nine types are available:</p>
<ul>
   <li>0: Displays a message (without the further need to perform a task).</li>
   <li>1: The participant has to move to fulfill the task.</li>
   <li>2: The participant has to rotate the floor.</li>
   <li>3: The participant has to enact (“animate”) an object.</li>
   <li>4: The participant has to leave (“disembody” from) an object.</li>
   <li>5: The participant has to use the action of an object (e.g., shaking the ground).</li>
   <li>6: The participant has to collect one or more notes.</li>
   <li>7: The participant has to press the “tab” key to show the controls.</li>
   <li>8: The participant has to hover the mouse cursor over the timer to display the remaining time.</li>
</ul>
<p>Example</p>
<p><code>“type”: 3</code></p>
<p><i>To complete this task, the participant has to enact an object. The type of object (e.g., cart, cloud) is determined by the variable “target”.</i></p>

<h4>3.2.3 “target”</h4>
<p>The target object to which the task refers. Needs to be specified for task types 1 and 3-6. Seven targets can be defined:</p>
<ul>
   <li>0: No target (to be selected if the task does not refer to any specific object).</li>
   <li>1: Unembodied, spherical form.</li>
   <li>2: The cloud.</li>
   <li>3: The ground.</li>
   <li>4: The handcar.</li>
   <li>5: The cart.</li>
   <li>6: All objects (task can be solved using any object).</li>
</ul>
<p>Example</p>
<p><pre>“type”: 1,
“target”: 4</pre></p>
<p><i>To complete this task, the participant has to move (type: 1) while having enacted the handcar (target: 4).</i></p>

<h4>3.2.4 “firstButton”, “secondButton”</h4>
<p>For tasks that require keyboard input, the buttons that need to be pressed are specified here. They are displayed at the bottom of the text box. The information is mandatory for tasks of type 1-5 and 7. The specification of the second key is optional. The following seven keys can be addressed:</p>
<ul>
   <li>0: Left arrow key (movement to the left, tilting the ground to the left).</li>
   <li>1: Upper arrow key (movement upwards, disembodiment from an object).</li>
   <li>2: Right arrow key (movement to the right, tilting the ground to the right).</li>
   <li>3: Lower arrow key (movement down, disembodiment from an object).</li>
   <li>4: Space bar (enacting an object, performing an action).</li>
   <li>6: Tab key (show controls).</li>
   <li>8: None (does not get displayed at the bottom of the text box).</li>
</ul>
<p>Example</p>
<p><pre>“type”: 5,
“target”: 2,
“firstButton”: 4,
“secondButton”: 8</pre></p>
<p><i>To complete this task, the participant has to press the spacebar (firstButton: 4) to perform the action (type: 5) of the cloud (target: 2), that is, the rain.</i></p>

<h4>3.2.5 “text”</h4>
<p>The contents of the text box and the description of the task. For tasks with multiple objectives (movement in two directions, collection of multiple notes), two or more terms can be placed in curly brackets to visualize the completion of the subtask.</p>
<p>Example</p>
<p><pre>“type”: 1,
“target”: 6,
“firstButton”: 0,
“secondButton”: 2,
“text”: “Move any object {left} and {right}.”</pre></p>
<p><i>Requires the movement to the left (firstButton: 0) and to the right (secondButton: 2). In the text box, the direction in which the arbitrary object (target: 6) is moved (type: 1) first is highlighted in green to indicate which subtask has already been completed.</i></p>

<h4>3.2.6 “amount”</h4>
<p>A value that, depending on the type of task, describes its duration, distance, angle or required number:</p>
<ul>
   <li>type 0 (message): The duration (in seconds) the message will be displayed.</li>
   <li>type 1 (move): The distance (in meters) that has to be covered by the objects.</li>
   <li>type 2 (rotate ground): The required rotation (in degrees, >0, <=15).</li>
   <li>type 3 (enactment), type 4 (disembodiment): No specification required.</li>
   <li>type 5 (perform action): The amount of time (in seconds) the space bar must be held down.</li>
   <li>type 6 (collect note): The number of notes to collect.</li>
   <li>type 7 (press tab): The duration (in seconds) that the tab key has to be held down (0, so it only has to be pressed once).</li>
   <li>type 8 (hover over timer): The duration that the mouse cursor has to hover over the timer (0, so that the mouse cursor only has to hover over the timer once).</li>
</ul>
<p>Example</p>
<p><pre>“type”: 0,
“target”: 0,
“firstButton”: 8,
“secondButton”: 8,
“text”: “This text box will be displayed for 4 seconds.”,
“amount”: 4.0</pre></p>
<p><i>Displays a message (type: 0) in a text box without a prompt (firstButton: 8; secondButton: 8) for four seconds (amount: 4.0). After that, the text box is closed and the task is considered completed.</i></p>

<h4>3.2.7 “activationCommands”, “completionCommands”</h4>
<p>Optional commands that are sent to and processed by the internal console on obtaining or completing the task. Multiple commands can be separated by the string “ \n “ (space, backslash, n, space). For an overview of the commands, please use the “help” function of the console in debug mode (see <a href="#2-7-debug">2.7 “debug”</a> in the description of the configuration file <i>moves.ini</i>). Also allows control of the hidden, computer-controlled player (“grey”) in the tutorial, e.g. to allow the collection of notes.</p>
<p>Example</p>
<p><pre>“activationCommands”: “hide notes”,
“completionCommands”: “show notes 2 \n grey enter cart”</pre></p>
<p><i>Hides the notes (instantly) when the task is received. After the task is completed, the notes are faded in over 2 seconds and the hidden grey player attempts to enact the cart.</i></p>

<h4>3.2.8 “types”, “targets”, “buttons”</h4>
<p>Three short summaries of “type”, “target” and “button” mappings within the configuration file. Serves only to give an overview when making adjustments.</p>

<h2 id="4-personalities">4. PERSONALITIES.INI (Configuration file)</h2>
<p>The configuration file <i>personalities.ini</i> allows to customize the artificial motivation of the computer controlled players.</p>

<h3>4.1 “version”</h3>
<p>The version of the <i>personalities.ini</i> file. Is intended to facilitate the management of various configurations. Shows up in the log files (see <a href="#5-log-files">5. Log files</a>).</p>

<h3>4.2 “personalities”</h3>
<p>An array of six objects that define the artificial motivation of the players. The first object contains the artificial motivation of the participant’s character and is disabled by default. By assigning an artificial motivation, the red player will also be controlled by the computer. The colors of the players are fixed: the first player is red, the second orange, the third yellow, the fourth green, the fifth blue and the sixth purple.</p>

<h4>4.2.1 “playerName”</h4>
<p>Specifying a name of the character only serves to make the file easier to read. Does not get processed by the MOVES program.</p>

<h4>4.2.2 “caption”, “color”</h4>
<p>The name or description of the player and the player’s color. Can be used inside the evaluation using placeholders (see <a href="2-9-2-instructions">2.9.2 “instructions”</a> and <a href="2-9-4-question">2.9.4 “question”</a>).</p>

<h4>4.2.3 “isActive”</h4>
<p>A boolean value that determines whether artificial motivation is active (true) or not (false). If the artificial motivation is disabled, the player will not make any automatic decisions.</p>
<p>Example</p>
<p><pre>“playerName”: “red (0)”,
“caption”: “RED”,
“color”: “red”,
“isActive”: false</pre></p>
<p><i>The red player is usually controlled by the participant. Therefore, the artificial motivation is turned off (isActive: false).</i></p>

<h4>4.2.4 “treatAs”</h4>
<p>Optional value used to match the overall artificial motivation of this character to that of another player. Accepts an integer value between 0 and 5, where 0 corresponds to the red player, 1 to the orange player, 2 to the yellow player, 3 to the green player, 4 to the blue player, and 5 to the purple player.</p>
<p>Example</p>
<p><pre>“playerName”: “red (0)”,
“isActive”: true,
“treatAs”: 2</pre></p>
<p><i>The red player is controlled by the computer and adopts the artificial motivation of the yellow player (treatAs: 2).</i></p>

<h4>4.2.5 “updateRoleInterval”</h4>
<p>The interval (in seconds) at which a random value is used to determine whether the player will attempt to enact another object. The default value is 2.</p>

<h4>4.2.6 “actionInterval”</h4>
<p>The interval (in seconds) at which a random value is used to determine whether the player will perform an action (e.g., rain as cloud, accelerate as handcar). The default value is 2.</p>

<h4>4.2.7 “minTimeBetweenDecisions”</h4>
<p>The minimum duration (in seconds) that must have passed since the last decision (see 4.2.5 “updateRoleInterval” and 4.2.6 “actionInterval”) so that a new decision (enacting another object, performing an action) can be made. The default value is 5. Serves to give the constellations a minimum degree of consistency so that the participant can react to them.</p>
<p>Example</p>
<p><pre>“updateRoleInterval”: 2,
“actionInterval”: 1,
“minTimeBetweenDecisions”: 8</pre></p>
<p><i>The computer checks every two seconds whether the player it controls intends to enact another object (updateRoleInterval: 2) and once every second, if he or she intends to use the action of his or her current object (actionInterval: 1). However, after enacting another object or performing an action, at least 8 seconds must have passed before enacting another object or performing an action becomes possible once again (minTimeBetweenDecisions: 8).</i></p>

<h4>4.2.8 “proximityLimit”</h4>
<p>The distance (in meters) that the player considers to be “close”. Used to control how close the cloud stays to the handcar while approach-motivated.</p>

<h4>4.2.9 “transitions”</h4>
<p>An array of seven objects that specify the probabilities of which object the artificially motivated player enacts when the participant 1) is unembodied, 2) has enacted the cloud, 3) has enacted the handcar, 4) has enacted the ground, 5) has enacted the cart, 6) disembodies from an object and returns to unembodied state, and at what probabilities the player 7) changes his or her mode of behavior.</p>

<h5>4.2.9.1 “other”</h5>
<p>Denotes the case to which the probabilities refer. The first object with the “other” value of “soul” describes the probabilities of enacting each of the objects while the participant is unembodied. The objects with the “cloud”, “handcar”, “ground”, and “cart” values describe the probabilities of enacting each of the objects while the participant is controlling the one specified at “other”. The object with the “other” value of “disembody” gives the probabilities of which object the player enacts when the participant disembodies from an object. The object with the “other” value of “actions” specifies the probabilities of the player changing from approach-motivated behavior to evocative or avoidance-motivated behavior as each of the four objects.</p>

<h5>4.2.9.2 “priority”</h5>
<p>Determines whether the player enacts an object even if it is currently enacted by the participant. Possible values are 0 (the player does not compete with the participant over this object) and 1 (the player takes over the object and kicks the participant out if necessary).</p>

<h5>4.2.9.3 “sampleCount”</h5>
<p>The probabilities are designed according to the sampling method, i.e. a behavior with a probability of 10% should always occur in exactly one out of ten cases. A low sample size leads to a high consistency in behavior, but is limited to coarse probability values (with a sample size of 5, probabilities can only be mapped in steps of 20%). A high sample size leads to more variance between runs. The default value is 20.</p>

<h5>4.2.9.4 “soul”, “cloud”, “handcar”, “ground”, “cart”</h5>
<p>Each of these contains an array of five probability values that should add up to 1. If the player controls the described object while the participant has enacted the object specified under “other”, it will, depending on the according probability, change to – or remain in – 1) the unembodied state, 2) the cloud, 3) the handcar, 4) the ground, or 5) the cart.</p>
<p>The object with the “other” value of “actions” is a special case. Here, the array “soul” is omitted and the remaining four arrays contain only two values. These determine the probability with which the corresponding object switches from approach-motivated behavior to 1) evocative or 2) avoidance-motivated behavior in suitable situations.</p>
<p>Example</p>
<p><pre>“transitions”: [
   {
      “other”: “cloud”,
      “priority”: 0,
      “soul”: [ 0, 0, 1, 0, 0 ],
      “cloud”: [ 1, 0, 0, 0, 0 ],
      “handcar”: [ 0, 0, 1, 0, 0 ],
      “ground”: [ 1, 0, 0, 0, 0 ],
      “cart”: [ 0, 0, 0.65, 0, 0.35 ]
   },
   {
      “other”: “actions”,
      “priority”: 0,
      “cloud”: [0, 0],
      “handcar”: [0.8, 0],
      “ground”: [0, 0],
      “cart”: [0, 0.65]
   }
]</pre></p>
<p><i>When the participant has enacted the cloud, the player defined by this artificial motivation behaves as follows: In unembodied state, he or she will animate the handcar 100% of the time (soul: [0,0,1(=handcar),0,0]). As the cloud, he or she will disembody to the unembodied state 100% of the time (cloud: [1(=unembodied),0,0,0,0]; this case is not possible anyway, since two players cannot control the cloud at the same time). As the handcar, he or she will always remain in it (handcar: [0,0,1(=handcar),0,0]). If currently controlling the ground, he or she will return to the unembodied state 100% of the time (ground: [1(=unembodied),0,0,0,0]). If  controlling the cart, he or she will switch to the handcar 65% of the time and stay in the cart 35% of the time (cart: [0,0,0.65(=handcar),0,0.35(=cart)]).</i></p>
<p><i>The object with the “other” value of “actions” indicates that the player will switch from approach-motivated behavior to evocative behavior 80% of the time as a handcar in appropriate situations (handcar: [0.8(=E),0]) and will exhibit avoidance-motivated behavior 65% of the time as a cart (cart: [0,0.65(=AV)]).</i></p>
<p><i>Note: Only two of the seven objects are shown to maintain clarity.</i></p>

<h2 id="5-log-files">5. Log files</h2>
<p>As soon as the participant begins an encounter with another player, all relevant actions of both characters are logged in the background until the time expires. Besides enacting and disembodying from objects, movement and collection of notes, the performed actions are logged with some situational context. When running on Windows, the log files are stored at the following path:</p>
<p><code>C:/Users/[USERNAME]/AppData/LocalLow/arne_sibilis/moves/logs/</code></p>

(...)

<h2 id="6-artificial-motivation">6. Artificial motivation tables</h2>
<p>Below is a detailed overview of all probabilities by which the artificial motivation described in chapter 4 are defined. The probability values for enacting and disembodying from objects are broken down by situation. Only objects that the player can potentially control are considered in the tables. Values highlighted in bold indicate that the corresponding change of enacted object can be performed even if the other player is currently controlling that object. Values in italics describe impossible situations and are listed only for the sake of completeness.</p>

(...)
