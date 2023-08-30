// 'esversion: 6'

// Remember: also change Assets\Packages\KomodoCore\Hidden~\WebGLTemplates\KomodoWebXRFullView2020\relay.js if you intend for the changes to be reflected in projects that use the package. If you are reading this from inside a build folder, you can change this file as you please.

// Tip: if you need to change this on-the-fly, you can edit this file without rebuilding. It's also possible to use the inspector to inspect the VR frame and call `window.RELAY_API_BASE_URL="<your-server-url>"`, if for some reason you need to do that in real time.

/* 
 * ---------------------------------------------------------------------------------
 * CONFIG
 * ---------------------------------------------------------------------------------
 */

// Replace these with your own server's URLs.

var RELAY_BASE_URL = "https://localhost:3000";//"https://appsocket.net:3000";//"https://localhost:3000";//"http://appsocket.net:3000"//"http://relay.appsocket.net"//http://appsocket.net:3000";//"3.232.172.204:3000";//"http://localhost:3000";
var API_BASE_URL = "http://localhost:4040";
var VR_BASE_URL = "http://localhost:8123"; //TODO -- change this to a better default

// init globals which Unity will assign when setup is done.
var sync = null;
var chat = null;

/* 
 * ---------------------------------------------------------------------------------
 * FUNCTIONALITY
 * ---------------------------------------------------------------------------------
 */

/**
 * Get the URL parameters
 * source: https://css-tricks.com/snippets/javascript/get-url-variables/
 * @param  {String} url The URL
 * @return {Object}     The URL parameters
 */
var getParams = function (url) {
    var params = {};
    var parser = document.createElement('a');
    parser.href = url;
    var query = parser.search.substring(1);
    var vars = query.split('&');
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split('=');
        params[pair[0]] = decodeURIComponent(pair[1]);
    }

    return params;
};

var params = getParams(window.location.href);

// Removes everything after the "?" in the input string.
var removeQuery = function (url) {
    var splitUrl = url.split("?");

    if (splitUrl.length != 2) {
        console.warn("Query was not removed.");

        return url;
    }

    return splitUrl[0];
};

// Replaces the hard-coded base URL
var removeVRBaseUrl = function (url) {
    var baselessURL = url.replace(VR_BASE_URL, "");

    if (baselessURL == url) {
        console.warn("Base URL was not removed.");

        return url;
    }

    return baselessURL;
};

var splitAppAndBuild = function (appAndBuild) {
    var splitString = appAndBuild.split("/");

    if(splitString.length != 4) {
        console.error("could not split app and build");
        
        return {app: appAndBuild, build: ""};
    }

    var app = splitString[1];

    var build = splitString[2];

    return {app: app, build: build};
};

var runtimeAppAndBuild = removeVRBaseUrl(removeQuery(window.location.href));

//console.log(runtimeAppAndBuild);

// TODO(Brandon): in the future, pass app and build as separate details like this:
// var result = splitAppAndBuild(runtimeAppAndBuild); 
// var runtimeApp = result.app;
// var runtimeBuild = result.build;
// ...then pass it in in request.onload below.

// Client and session params supplied by portal iframe src
// var session_id = Number(params.session);
var session_id = 0;
// var client_id = Number(params.client);
var isTeacher = Number(params.teacher) || 0;
var playback_id = Number(params.playback);

// wrapper `details` object to be populated by api call and passed to Unity
// NOTE(rob): Unity cannot deserialize raw arrays
// NOTE(Brandon): "build" here refers to the lab configuration set by the instructor
// ... build may differ at runtime for admin and instructor users
var details = {
    assets: [],
    build: "",
    course_id: 0,
    create_at: "",
    description: "",
    end_time: "",
    session_id: 0,
    session_name: "",
    start_time: "",
    users: []
};

// fetch lab details from portal api
// var url = API_BASE_URL + "/labs/" + session_id.toString();
// var request = new XMLHttpRequest();
// request.open("GET", url, true);
// request.responseType = "json";
// request.send();

// request.onload = function(){
//     let res = request.response;

//     // session details
//     details.build = runtimeAppAndBuild;
//     details.course_id = res.course_id;
//     details.create_at = res.create_at;
//     details.description = res.description;
//     details.end_time = res.end_time;
//     details.session_id = res.session_id;
//     details.session_name = res.session_name;
//     details.start_time = res.start_time;
//     details.users = res.users;
    
//     let assets_response = res.assetList;
//     for (idx = 0; idx < assets_response.length; idx++)
//     {
//         asset = new Object;
//         asset.id = assets_response[idx].asset_id;
//         asset.name = assets_response[idx].asset_name;
//         asset.url = assets_response[idx].path;
//         asset.isWholeObject = Boolean(assets_response[idx].is_whole_object);
//         asset.scale = assets_response[idx].scale || 1;
//         details.assets.push(asset);
//     }
// };
