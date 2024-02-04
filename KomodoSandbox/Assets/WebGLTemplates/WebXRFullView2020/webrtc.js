let localStream;
let peerConnection;
let socket;
let isMicrophoneMuted = false; // Tracks the microphone state
let isVideoShared = true; // Assuming video starts as shared

let isInCall = false;
let peerConfiguration = {
    iceServers: [
        {
            urls: [
                'stun:stun.l.google.com:19302',
                'stun:stun1.l.google.com:19302'
            ]
        }
    ]
}

let currentClientOffersMap = new Map();
//if(!RELAY_BASE_URL)
//let RELAY_BASE_URL = 'https://192.168.1.67:3000';

const constraints = {
    video: {
        width: { max: 256 },  // Maximum width
    //    height: { max: 192 }, // Maximum height
        aspectRatio: { ideal: 1.33 } // Optional: Ideal aspect ratio (16:9 in this case)
    },//true,
    audio: true
};
// Create a new MediaStream
var remoteStream = new MediaStream();
var remoteVideoEl = document.getElementById('remoteVideo');
var localVideo = document.getElementById('localVideo');

let roomName = null;
let clientsInCall = [];

let offers = []; // This should be your actual list of offers

//const userName = "Rob-" + Math.floor(Math.random() * 100000)
let userName = "Rob-" + Math.floor(Math.random() * 100000);
const password = "x";

// let offeredClients = new Set();
let didIOffer = false;

let isScreenSharing = false;

//remove this when adding to unity
// document.addEventListener('DOMContentLoaded', () => {
//     ConnectToWebRTCSocket(0, "Rob-" + Math.floor(Math.random() * 100000));
// });

function ConnectToWebRTCSocket(name) {

    console.log(`ConnectToWebRTCSocket : ${name}`);

    userName = name;

    if (!socket) {
        socket = io(RELAY_BASE_URL, {
            auth: {
                userName, password, client_id: window.client_id
            }
        });

        setupSocketListeners();

        document.querySelector('#currentClientName').textContent = userName;

    }
}

function setupSocketListeners() {

    socket.on('availableOffers', offers => {
        this.offers = offers;
        updateOfferElements(offers);
    });

    //invoked on client that is being called
    socket.on('newOfferAwaiting', data => {

        console.log(`newOfferAwaiting : ${data.newOffer.offererUserName}`);

        currentClientOffersMap.set(data.newOffer.offererUserName, data.newOffer);

        if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientCall', data.offererClientID);


        addToOffers(data.newOffer);
    });

    socket.on('removeOffer', (offer) => {
       console.log('removeOffer-------------', offer);

        removeOffer(offer);
    });

    socket.on('roomCreated', (data) => {
//        console.log(`roomCreated : ${data.roomName} with: ${data.nameToAdd}`);

        roomName = data.roomName;

        clientsInCall.push(data.nameToAdd);
    });

    socket.on('answerResponse', data => {
        addAnswer(data.offer);

        if (window && window.gameInstance && window.socketIOAdapterName)
        window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientAnswer', data.offererClientID);

    });



    socket.on('receivedIceCandidateFromServer', iceCandidate => {
        addNewIceCandidate(iceCandidate);
    });


    socket.on('clientsUpdate', clients => {

        //availableClients = clients;
        console.log(`Available clients: ${clients}`);

        updateClientElements(clients);
    });

    socket.on('addClient', clients => {

        //availableClients = clients;
       // console.log(`Available clients: ${clients}`);

        addClientElement(clients);
    });

    socket.on('callEnded', () => {
        // Handle the call end event

        if (window && window.gameInstance && window.socketIOAdapterName){
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveCallEnded', userName);
             console.log(`SENDMESSAGE RECEIVECALLENDED : ${userName}`);
        }

        
        endCall(0);

       
    });


    socket.on('clientDisconnected', (userName) => {
        // Handle the call end event
       // console.log(`clientDisconnected : ${userName}`);
       // clientDisconnected(userName);

       
        
        endCall(0);
    });

    socket.on('acceptClientOffer', (data) => {
        // Handle the call end event


        acceptClientOffer(data.offer, data.isAnswerer);
    //    addAnswer(offer);
    //     console.log(`acceptClientOffer : ${offer}`);
    //     answerOffer(offer);
    });





}

async function acceptClientOffer(offer, isAnswerer) {

    if(!isAnswerer){
    socket.emit('offerAnswered', offer.offererUserName);
    await answerOffer(offer);
    }else{
        if (window && window.gameInstance && window.socketIOAdapterName)
        window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientAnswer', data.offererClientID);
    }

    // if(isAnswerer)
    // await peerConnection.setLocalDescription(offer);

    // console.log(`acceptClientOffer : ${offer}`);
 
    // await answerOffer(offer);

    // Write the answer to the offer
    // offer.answer = answer;

    // // Add the answered offer
    // await addAnswer(offer);
}
// function addClientElement(userName) {


//     // Proceed with the rest of the function
//     const clientsEl = document.getElementById('clients');
//     if (clientsEl) {

//                 const newClientEl = document.createElement('div');
//                 newClientEl.innerHTML = `<button id=call-${userName} style="position: relative; z-index: 1000;" class="btn btn-success col-1">Call ${userName}</button>`;
//                 newClientEl.addEventListener('click', () => {
//                     call(client);
//                 });
//                 clientsEl.appendChild(newClientEl);

//     } else {
//         console.error('Element with ID "clients" not found');
//     }
// }


function updateClientElements(clients) {

    // Proceed with the rest of the function
    const clientsEl = document.getElementById('clients');
    if (clientsEl) {
        clientsEl.innerHTML = '';
        clients.forEach(client => {

            if (client !== userName) {
                const newClientEl = document.createElement('div');
                newClientEl.innerHTML = `<button id=call-${client} style="position: relative; z-index: 1000;" class="btn btn-success col-1">Call ${client}</button>`;
                newClientEl.addEventListener('click', () => {
                    call(client);
                });
                clientsEl.appendChild(newClientEl);
            }
            // Rest of the code to handle each client...
        });
    } else {
        console.error('Element with ID "clients" not found');
    }
}


function addToOffers(offer) {

    // Update the DOM
const answerEl = document.querySelector('#answer');
if (answerEl) {
       const newOfferEl = document.createElement('div');
       newOfferEl.innerHTML = `<button id="${offer.offererUserName}" class="btn btn-success col-1">Answer ${offer.offererUserName}</button>`;
       newOfferEl.addEventListener('click', async () => {
           //socket.emit('offerAnswered', offer);
           await answerOffer(offer);
           newOfferEl.remove();
        });
        answerEl.appendChild(newOfferEl);
} else {
   console.error('Element with id "answer" not found');
}
}

async function answerClient(offererUserName) {


    const offer = currentClientOffersMap.get(offererUserName);
    console.log(`answerClient : ${offererUserName}`);
    console.log(`answerClient : ${offer}`);
    console.log(`answerClient client: ${currentClientOffersMap.size}`);
    //currentClientOffersMap.length
   // socket.emit('offerAnswered', offer);
   await answerOffer(offer);

    removeOffer(offer);

 }

function removeOffer(offer) {

    if(!offer)
    return;

    if(currentClientOffersMap.has(offer.offererUserName))
    currentClientOffersMap.delete(offer.offererUserName);

const offerEl = document.getElementById(offer.offererUserName);

if (offerEl) {
    offerEl.remove();
}else {
    console.warn('Element with id not found');
 }

}

const endButton = document.getElementById('endButton');
endButton.addEventListener('click',()=> endCall(1));

function endCall(isCaller) {
  //  isInCall = false;
    clientsInCall.forEach(name => {

        const clientButton = document.getElementById(`call-${name}`);

        if (clientButton) clientButton.style.display = 'block';

       // document.getElementById(`call-${name}`).style.display = 'block';
    });

if(isCaller){
    console.log("roomname: " + roomName);
    socket.emit('sendCallEndedToServer', roomName);

}

    if (peerConnection) {
        peerConnection.close();
        //peerConnection = null;
        localStream.getTracks().forEach(track => track.stop());

        clientsInCall = [];

        // Reset the room name
        roomName = null;
    }
//    remove
}

// function endAllConnections() {

//    // isInCall = false;
//     clientsInCall.forEach(name => {
//         document.getElementById(`call-${name}`).style.display = 'block';



//      });

//     if (peerConnection) {
//         peerConnection.close();
//        // peerConnection = null;
//         localStream.getTracks().forEach(track => track.stop());

//         clientsInCall = [];

//         // Reset the room name
//         roomName = null;
//     }

// }

function clientDisconnected(userName) {

        // if(!isInCall)
        // return;
        
        console.log(`clientDisconnected : ${clientsInCall}`);

   
        endCall(0);


}




function updateOfferElements(newOffers) {

     // Filter and update the offers
//offers = offers.filter(offer => newOffers.some(newOffer => newOffer.offererUserName === offer.offererUserName));

newOffers.forEach(newOffer => {
    if (newOffer.offererUserName !== userName) {
        const existingOfferIndex = offers.findIndex(offer => offer.offererUserName === newOffer.offererUserName);
        if (existingOfferIndex !== -1) {
            offers[existingOfferIndex] = newOffer;
        } else {
            offers.push(newOffer);
        }
    }
});

// Update the DOM
const answerEl = document.querySelector('#answer');
if (answerEl) {
    answerEl.innerHTML = '';
    offers.forEach(o => {
        const newOfferEl = document.createElement('div');
        newOfferEl.innerHTML = `<button id="${o.offererUserName}" class="btn btn-success col-1">Answer ${o.offererUserName}</button>`;
        newOfferEl.addEventListener('click', async () => {
            newOfferEl.remove();
          //  offers = offers.filter(offer => offer.offererUserName !== o.offererUserName);
            socket.emit('offerAnswered', o.offererUserName);
            await answerOffer(o);
        });
        answerEl.appendChild(newOfferEl);
    });
} else {
    console.error('Element with id "answer" not found');
}
}




async function answerOffer(offer) {
    await fetchUserMedia();
    await createPeerConnection(offer);
    const answer = await peerConnection.createAnswer();
    await peerConnection.setLocalDescription(answer);

    offer.answer = answer;
    offer.answererUserName = userName;

    const offerIceCandidates = await socket.emitWithAck('answerResolve', offer);
    offerIceCandidates.forEach(c => {

  if (peerConnection && peerConnection.signalingState !== 'closed')
        peerConnection.addIceCandidate(c);
    });

   // return answer;
}

function addNewIceCandidate(iceCandidate) {
     if (iceCandidate && peerConnection && peerConnection.signalingState !== 'closed')
     peerConnection.addIceCandidate(new RTCIceCandidate(iceCandidate));
}

async function addAnswer(offer) {
    await peerConnection.setRemoteDescription(new RTCSessionDescription(offer.answer));
}

async function createPeerConnection(offerObj) {
    peerConnection = new RTCPeerConnection(peerConfiguration);
    remoteStream = new MediaStream();
    remoteVideoEl.srcObject = remoteStream;
    localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));

    peerConnection.onicecandidate = e => {
        if (e.candidate) {
            socket.emit('sendIceCandidateToSignalingServer', {
                iceCandidate: e.candidate,
                iceUserName: userName,
                didIOffer
            });
        }
    };

    peerConnection.ontrack = e => {
        e.streams[0].getTracks().forEach(track => remoteStream.addTrack(track));
    };

    if (offerObj) {
        await peerConnection.setRemoteDescription(new RTCSessionDescription(offerObj.offer));
    }
}
// Flag to indicate screen sharing state
async function fetchUserMedia(includeVideo = true) {

    const mediaConstraints = {
        audio: true,
        video: includeVideo ? constraints.video : false
    };

    try {
        originalStream = await navigator.mediaDevices.getUserMedia(mediaConstraints);
        localVideo.srcObject = originalStream;
        localStream = originalStream;
    } catch (videoError) {
        // if (videoError.name === "NotAllowedError" || videoError.name === "PermissionDeniedError") {
            // Video permission denied, try to get audio only
            try {
                const audioOnlyStream = await navigator.mediaDevices.getUserMedia({ audio: true });
                localStream = audioOnlyStream;
                // Note: No video element source for audio-only stream
                // Update UI accordingly, if necessary
            } catch (audioError) {
                console.error('Error accessing audio:', audioError);
                // Handle the case where neither video nor audio is accessible
                // Update UI to inform the user
            }

    }

//}
}


async function call(sendToUserName) {
    console.log(`call : ${sendToUserName}`);
    await fetchUserMedia();
    await createPeerConnection();

    try {
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
        didIOffer = true;
        // Emitting the offer to the signaling server with details of the offerer, answerer, and any additional data required
        socket.emit('newOffer', { offer, offererUserName: userName, answererUserName: sendToUserName});
    } catch (error) {
        console.error('Error creating offer:', error);
    }
}

document.getElementById('startScreenSharingButton').addEventListener('click',
async () => {
   // await fetchUserMedia(); // Refresh original stream
   document.getElementById('stopScreenSharingButton').removeAttribute('disabled');
   document.getElementById('startScreenSharingButton').setAttribute('disabled', false);
    await startScreenSharing();
}
);
document.getElementById('stopScreenSharingButton').addEventListener('click', ()=>{

    document.getElementById('stopScreenSharingButton').setAttribute('disabled',false);
    document.getElementById('startScreenSharingButton').removeAttribute('disabled');
    stopScreenSharing();
});


let screenStream;
let originalStream; // This will store the original user media stream

async function startScreenSharing() {
    if (!isScreenSharing) {
        try {
            screenStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
            const screenTrack = screenStream.getVideoTracks()[0];
            const sender = peerConnection.getSenders().find(s => s.track.kind === 'video');
            if (sender) {
                await sender.replaceTrack(screenTrack);
            }
            localVideo.srcObject = screenStream;
            localStream = screenStream; // Update for signaling
            isScreenSharing = true;
        } catch (error) {
            console.error('Error starting screen sharing:', error);
        }
    }
}


async function stopScreenSharing() {
    // Stop screen sharing stream tracks
    if (screenStream) {
        screenStream.getTracks().forEach(track => track.stop());
    }

    // Check and replace track only if in screen sharing mode
    if (isScreenSharing && originalStream) {
        const videoTrack = originalStream.getVideoTracks()[0];
        const sender = peerConnection.getSenders().find(s => s.track.kind === 'video');
        if (sender && videoTrack) {
            await sender.replaceTrack(videoTrack);
            localVideo.srcObject = originalStream;
            localStream = originalStream; // Ensure localStream is set back to original
            console.log('Screen sharing stopped, and original video track restored.');
            isScreenSharing = false; // Reset screen sharing flag
        } else {
            console.error('Failed to find video sender or original video track.');
        }
    } else {
        console.error('No original stream available or not in screen sharing mode.');
    }

    // Refresh originalStream for future use
    await fetchUserMedia();
}

// async function createAndSendOffer(remotePeerUserName) {
//     try {
//         const offer = await peerConnection.createOffer();
//         await peerConnection.setLocalDescription(offer);
//         // Use the remotePeerUserName as part of the offer message
//         socket.emit('newOffer', { offer, offererUserName: userName, answererUserName: remotePeerUserName, type: 'offer' });
//     } catch (error) {
//         console.error('Error creating offer:', error);
//     }
// }

// document.getElementById('startAudioCallButton').addEventListener('click', () => {
//     fetchUserMedia(false); // Start an audio-only call
// });

document.getElementById('toggleMicrophoneButton').addEventListener('click', MuteMicToggle);

function MuteMicToggle(){

// Assuming localStream is already acquired and includes audio tracks
if (localStream && localStream.getAudioTracks().length > 0) {
    isMicrophoneMuted = !isMicrophoneMuted; // Toggle the state
    localStream.getAudioTracks().forEach(track => track.enabled = !isMicrophoneMuted);
}
}

document.getElementById('toggleVideoButton').addEventListener('click', ShareVideoToggle);

function ShareVideoToggle()
{

       // Assuming localStream is already acquired and includes video tracks
       if (localStream && localStream.getVideoTracks().length > 0) {
        isVideoShared = !isVideoShared; // Toggle the state
        localStream.getVideoTracks().forEach(track => track.enabled = isVideoShared);
    }
}

