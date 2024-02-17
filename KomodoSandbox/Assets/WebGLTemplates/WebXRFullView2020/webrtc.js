let localStream;

let videoElements = [];//new Map();

let videoElementsMap = new Map();
//peer connection setup
let connectedPeers = new Set(); // Tracks peers we're connected to


let connectedPeersMap = new Map(); // Tracks peers we're connected to


let peerConnections = new Map();

let socket;
let isMicrophoneMuted = true; // Tracks the microphone state
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
var localVideo = document.getElementById('localVideo');

let roomName = null;
let clientsInCall = [];

let offers = []; // This should be your actual list of offers

//const userName = "Rob-" + Math.floor(Math.random() * 100000)
let userName = "Rob-" + Math.floor(Math.random() * 100000);
const password = "x";

// let offeredClients = new Set();
//let didIOffer = false;

let isScreenSharing = false;

//remove this when adding to unity
// document.addEventListener('DOMContentLoaded', () => {
//     ConnectToWebRTCSocket("Rob-" + Math.floor(Math.random() * 100000));
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

    // socket.on('updateOffersInMap', offer => {

    //     currentClientOffersMap.set(offer.offererUserName, offer);
    //     // this.offers = offers;
    //     // updateOfferElements(offers);
    // });

    //invoked on client that is being called
    socket.on('newOfferAwaiting', data => {

        console.log(`newOfferAwaiting : ${data.newOffer.offererUserName}`);

        currentClientOffersMap.set(data.newOffer.offererUserName, data.newOffer);

        //socket.emit('updateOffersInMap', data.newOffer) //socket.io.

        if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientCall', data.offererClientID);


        addToOffers(data.newOffer);
    });

    socket.on('newOfferAwaiting2', data => {

        console.log(`newOfferAwaiting : ${data.newOffer.offererUserName}`);

  //      if(!data.isForClientSync)
        currentClientOffersMap.set(data.newOffer.offererUserName, data.newOffer);

        //socket.emit('updateOffersInMap', data.newOffer) //socket.io.

        if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientCall', data.offererClientID);


        addToAutomaticClickedOffers({offer: data.newOffer, offererSocketID : data.offererClientID});
        //addToOffers(data.newOffer);
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

    socket.on('answerResponse', async (data) => {

        if(connectedPeers.has(data.offer.answererUserName)) //connectedPeersMap.has(data.offer.answererUserName))
        {
            console.log(`This User: ${userName} is already connected to ${data.offer.answererUserName} aborting answerResponse.`);
            return;
        }

        console.log(`answerResponse : ${data.offer.answererUserName}`);

        
        
        let peerConnection = await getOrCreatePeerConnection(data.offer.answererUserName);//createPeerConnection(offer.answererUserName);

        data.offer.offererUserName = userName;
        //attachIceCandidateListener(peerConnection, data.offer.answererUserName, false);

        // if(peerConnection)
        // await peerConnection.setRemoteDescription(new RTCSessionDescription(data.offer.answer));
        //addAnswer(data.offer);

        if (peerConnection.signalingState === 'stable') {
            console.log('Cant set Remote Pranser Description in stable state. Return.');
            return;
        } else {
            await peerConnection.setRemoteDescription(new RTCSessionDescription(data.offer.answer));
          //  console.log('The connection is not stable.');
        }


      
  //if(!isForSync)
    if(connectedPeers.has(data.offer.answererUserName)) //connectedPeersMap.has(data.offer.answererUserName))
    {
        console.log(`This User: ${userName} is already connected to ${data.offer.answererUserName} aborting.`);
     //   return;
    }else{

         //  connectedPeersMap.set(data.offer.offererUserName, data.offer.offererSocketID);
        // connectedPeersMap.set(userName, socket.id);
        console.log(`OFFERER CONNECTED PEERS:  ${connectedPeersMap.size})`)
        
        socket.emit('roomCallClient',data.offer.offererUserName, Array.from(connectedPeersMap.values()));
        connectedPeersMap.set(data.offer.answererUserName, data.offer.answererSocketID);

    }

   // currentClientOffersMap.set(data.newOffer.offererUserName, data.newOffer);

   if (window && window.gameInstance && window.socketIOAdapterName)
   window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientAnswer', data.offererClientID);




       
    });

    socket.on('answerResponse2', data => {

        console.log(`answerResponse2 : ${data.offer.answererUserName}`);
        // if (!peerConnection) {
        //     peerConnection  = await createPeerConnection( null,userName);
        //     peerConnections.set(userName, peerConnection);
        // }
        //if(data.offer.answer)
        //if(!data.offer.isForClientSync)
        //addAnswer(data.offer);



        if (window && window.gameInstance && window.socketIOAdapterName)
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientAnswer', data.offererClientID);

    });



    // socket.on('receivedIceCandidateFromServer', candidate => {
    //     addNewIceCandidate(candidate.iceCandidate, candidate.offer);
    // });
    socket.on('receivedIceCandidateFromServer', async (candidate) => {
        // addNewIceCandidate(candidate.iceCandidate, candidate.offer, candidate.to);
        console.log(`receivedIceCandidateFromServer : ${userName}`)

        const peerConnection = await getOrCreatePeerConnection(candidate.from);//offer.answererUserName);

        if (candidate.iceCandidate && peerConnection && peerConnection.remoteDescription && peerConnection.signalingState !== 'closed')
            peerConnection.addIceCandidate(new RTCIceCandidate(candidate.iceCandidate));
        else {
            console.log("Remote description not set. Buffering ICE candidate.");
        }

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

    socket.on('callEnded', clientID => {
        // Handle the call end event

        if (window && window.gameInstance && window.socketIOAdapterName) {
            window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveCallEnded', clientID);
            console.log(`SENDMESSAGE RECEIVECALLENDED : ${userName}`);
        }


        endCall(0);


    });


    socket.on('clientDisconnected', (disconectingUserName) => {
        // Handle the call end event


        endCall(0, disconectingUserName);
    });

    socket.on('acceptClientOffer', (data) => {

        acceptClientOffer(data);

    });

    socket.on('rejectedClientOffer', (data) => {

        console.log(`rejectedClientOffer : ${data}`);
        //  removeOffer(data);

    });
















    // Assuming this function gets called when a new peer needs to be connected
    // socket.on('connectWithNewPeer', async (newPeerUserName) => {


    //     call(newPeerUserName, true);
    //     // await fetchUserMedia();

    //     // if(newPeerUserName === userName || connectedPeers.has(newPeerUserName)){
    //     //     console.log(`Connection to ${newPeerUserName} is already established or in progress.`);
    //     //     return;
    //     // }


    //     // // if (!connected.includes(newPeerUserName)) {

    //     // console.log(`Initiating connection with new peer: ${newPeerUserName}`);

    //     //  // if (!peerConnections.has(newPeerUserName)) {

    //     //     const peerConnection = await getOrCreatePeerConnection(newPeerUserName, true);

    //     //     // Create an offer to the new peer
    //     //     const offer = await peerConnection.createOffer();

    //     //     console.log(`LOCAL ---- NEW PEER CONNECTED BEFORE-_ OFFER DESCRIPTION : ${newPeerUserName}` + peerConnection.signalingState);
    //     //     await peerConnection.setLocalDescription(offer);
    //     //     console.log(`LOCAL ---- NEW PEER CONNECTED AFTER-_ OFFER DESCRIPTION : ${newPeerUserName}` + peerConnection.signalingState);


    //     //     // connectedPeers.add(newPeerUserName);

    //     //     // offer.isForClientSync = true;

    //     //     socket.emit('newOffer', {
    //     //         offer,
    //     //         offererUserName: userName,   //from
    //     //         answererUserName: newPeerUserName, //to
    //     //     //  didIOffer,
    //     //        // isAnsweringCall: true,
    //     //     });

    //     // }else{

    //     //     console.log(`Peer already exists: ${newPeerUserName}`);
    //     // }
    //     // }
    // });

    // Listen for an event that carries an incoming offer
    // socket.on('receiveOffer', async (newOffer, fromUserName, offererClientID) => {

    //     console.log(`Received offer from ${fromUserName} : ${newOffer.offer}`);
    //     await handleIncomingOffer(newOffer, newOffer.offererUserName);
    // });

    // let connected = []
    // // Handling the incoming offer and automatically sending an answer
    // async function handleIncomingOffer(newOffer, fromUserName) {

    //     // connectedPeers.add(newPeerUserName);

    //     //if (!peerConnections.has(fromUserName)) {

    //     const peerConnection = await getOrCreatePeerConnection(fromUserName);

    //     console.log(`REMOtE --- NEW PEER CONNECTED BEFORE-_ OFFER DESCRIPTION : ${fromUserName}` + peerConnection.signalingState);

    //     //Have local offer


    //     await answerOffer2(newOffer);
    //     return;
    //     await peerConnection.setRemoteDescription(new RTCSessionDescription(newOffer.offer));

    //     console.log(`REMOtE --- NEW PEER CONNECTED AFTER OFFER DESCRIPTION : ${fromUserName}` + peerConnection.signalingState);
    //     //if (peerConnection.signalingState === "stable") {

    //     //createPeerConnection sets this up when adding an offer to it in its arguments

    //     const answer = await peerConnection.createAnswer();
    //     await peerConnection.setLocalDescription(answer);

    //     newOffer.answer = answer;

    //     // //dont invoke it for other clients
    //     // newOffer.isForClientSync = true;


    //     const offerResult = await socket.emitWithAck('answerResolve2', newOffer);

    //     offerResult.offerIceCandidates.forEach(c => {

    //         if (peerConnection && peerConnection.signalingState !== 'closed')
    //             peerConnection.addIceCandidate(c);
    //     });



    //     //  newOffer.answer = answer;

    //     // connected.push(offer.answererUserName);
    //     //  console.log(fromUserName);
    //     // Send the answer back to the peer who sent the offer
    //     socket.emit('sendAnswer', { offer: newOffer, fromUserName, user: userName });



    //     // }else {
    //     // console.log(`___+++++____IN handleIncomingOffer Cannot set remote offer for ${fromUserName} in signalingState: ${peerConnection.signalingState}`);

    //     // if (peerConnection.signalingState === "have-local-offer"){


    //     // }
    //     // }

    // }

    // socket.on('receiveAnswer2', async (offer, fromUserName, user) => {
    //     console.log(`Received answer from ${fromUserName}.` + offer);

    //     //connectedPeers.add(fromUserName);
    //     //   connected.push(fromUserName);


    //     await handleRemoteAnswer(offer, fromUserName);
    // });

    // async function handleRemoteAnswer(offer, fromUserName) {


    //     if (fromUserName === userName) {
    //         console.log(`SAME PEER AS USER FOUND RETURN : ${fromUserName}`);
    //         return;
    //     }


    //     const peerConnection = await getOrCreatePeerConnection(offer.offererUserName);


    //     // if (peerConnection.signalingState === "have-local-offer") {

    //     // console.log(offer);
    //     //      await peerConnection.setRemoteDescription(new RTCSessionDescription(offer.answer));
    //     //  console.log("Answer set as the remote description.");
    //     // connected.push(offer.answererUserName);

    //     // } else {
    //     //     console.log(`___+++++____IN handleRemoteAnswer Cannot set remote offer for ${fromUserName} in signalingState: ${peerConnection.signalingState}`);
    //     // }
    //     // }
    // }

    socket.on('makeClientSendOffer', (clientToAdd) => {

        console.log(`makeClientSendOffer : ${clientToAdd}`);
        //call(clientToAdd, true);
        call(clientToAdd, true);

    });

















}

async function acceptClientOffer({ offer, offererClientID }) {

    //  if(!isAnswerer){
    socket.emit('offerAnswered', offer.offererUserName);
    //   socket.emit('offerAnswered', offer.answererUserName);

    console.log(`++++acceptClientOffer : ${offer.offererUserName}`);
    await answerOffer(offer);




    //if(offer.isAnswerer)
    if (window && window.gameInstance && window.socketIOAdapterName)
        window.gameInstance.SendMessage(window.socketIOAdapterName, 'ReceiveClientAnswer', offererClientID);


}



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
            console.log(`___________addToOffers : ${offer.offererUserName}`);
            await answerOffer(offer);
            newOfferEl.remove();
        });
        answerEl.appendChild(newOfferEl);
    } else {
        console.error('Element with id "answer" not found');
    }
}

async function addToAutomaticClickedOffers({offer, offererSocketID  }) {

    await answerOffer(offer, offererSocketID, true);
}

async function answerClient(offererUserName) {


    console.log(`answerClient (function parameter) : ${offererUserName}`);

    // if(!offererUserName)
    // return;
    const offer = currentClientOffersMap.get(offererUserName);
    // if(!offer)
    // return;
    console.log(`answerClient (existance in map) : ${offer}`);


    console.log(`answerClient client: ${currentClientOffersMap.size}`);
    //currentClientOffersMap.length
    // socket.emit('offerAnswered', offer);
    await answerOffer(offer);

  //  removeOffer(offer);

    // currentClientOffersMap.delete(offererUserName);

}

function removeOffer(offer) {

    if (!offer)
        return;

    // if(currentClientOffersMap.has(offer.offererUserName))
    // currentClientOffersMap.delete(offer.offererUserName);

    const offerEl = document.getElementById(offer.offererUserName);


    if (offerEl) {
        offerEl.remove();
    } else {
        console.warn('Element with id not found');
    }

}

const endButton = document.getElementById('endButton');
endButton.addEventListener('click', () => endCall(1));

async function endCall(isCaller, disconnectingUserName) {

    //clientsInCall contais the names of the clients alternative to the user. Answerer -> Offerer / Offerer -> Answeret
    clientsInCall.forEach(name => {

        const clientButton = document.getElementById(`call-${name}`);

        if (clientButton) clientButton.style.display = 'block';

    });


    if (disconnectingUserName) {

        clientsInCall = clientsInCall.filter(client => client !== disconnectingUserName);


        let peerConnection = peerConnections.get(disconnectingUserName);

        if (peerConnection) {
            peerConnection.close();
            peerConnections.delete(disconnectingUserName);
            peerConnection = null;

            let videoElementId = "remoteVideo_" + disconnectingUserName;
            let videoElement = document.getElementById(videoElementId);

            if (videoElement)
                videoElement.parentNode.removeChild(videoElement);

        }

    }

    // localStream.getTracks().forEach(track => track.stop());
    // clientsInCall = [];
    // roomName = null;

    if (isCaller) {
        //  console.log("roomname: " + roomName);
        socket.emit('sendCallEndedToServer', userName);
    }

}


function clientDisconnected(userName) {

    console.log(`clientDisconnected : ${clientsInCall}`);

    endCall(0, userName);
}

function rejectOffer(clientID) {
    // Construct a rejection message
    const rejectionMessage = {
        type: 'offer-rejection',
        //  offererUserName: offererUserName,
        // offererSocketID: offererSocketID,
        offererClientID: clientID,
        answererUserName: userName,
        reason: 'Busy' // Optional, you can provide a reason
    };

    // Send the rejection message to the signaling server
    socket.emit('requestRejectOffer', rejectionMessage);
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


//let currentOffererUserName;

async function answerOffer(offer, offererSocketID, isForSync) {

//    if( connectedPeers.has(offer.offererUserName)){
//     console.log(`This User: ${userName} is already connected to ${offer.offererUserName} in answerOffer answer.`);
  
//     return;
//    }
    // if(connectedPeers.has(offer.offererUserName)){//connectedPeersMap.has(sendToUserName)){

    //     console.log(`This User: ${userName} is already connected to ${sendToUserName} aborting answer.`);
    //     return;
    // }
    

//     if(offererSocketID)
//     {
//         socket.emit('roomCallClient',offer.offererUserName, Array.from(connectedPeersMap.values()), offer.offererSocketID, offer);

//    }
    
    
    //call this for the offerer socket aswell getting to offerer as someone needed to be exposed. this is done
    //since the sole offerer that does not know about peers is the one that is calling the answerer 
    //with multi peers



    console.log(`answerOffer : ${offer}`);

    let peerConnection = await getOrCreatePeerConnection(offer.offererUserName)//createPeerConnection(offer.offererUserName);

    await peerConnection.setRemoteDescription(new RTCSessionDescription(offer.offer));


    const answer = await peerConnection.createAnswer({});

    await peerConnection.setLocalDescription(answer);

    offer.answer = answer;
  offer.answererUserName = userName;


    let offerResult = null;

    offer.clientsInCall = clientsInCall;


    offerResult = await socket.emitWithAck('answerResolve', offer);




    




    offerResult.offerIceCandidates.forEach(c => {

        if (peerConnection && peerConnection.signalingState !== 'closed')
            peerConnection.addIceCandidate(c);

        console.log(`${userName}  ======Added Ice Candidate======`)
    });


    console.log(`ANSWERER CONNECTED PEERS:  ${connectedPeersMap.size})`)
    //if(offererSocketID)
  
     //if(!isForSync)
      if(connectedPeers.has(offerResult.offererUserName)) //connectedPeersMap.has(offerResult.offererUserName))
      {
          console.log(`This User: ${userName} is already connected to ${offerResult.offererUserName} in answerOffer answer.`);
       //  return;
      }else{
        
          connectedPeersMap.set(offerResult.offererUserName, offerResult.offererSocketID);
          socket.emit('roomCallClient', offerResult.answererUserName, Array.from(connectedPeersMap.values()));
          
      }


  //  if(isForSync)
  // else
  //   connectedPeersMap.set(offer.offererUserName, offer.offererSocketID);
  
  
 
}


// async function answerOffer2(offer) {


//     //await fetchUserMedia();

//     console.log(`answerOffer : ${offer}`);

//     // let peerConnection = peerConnections.get( offer.offererUserName);

//     //     if (!peerConnection) {
//     let peerConnection = await getOrCreatePeerConnection(offer.offererUserName)//createPeerConnection(offer.offererUserName);
//     //        peerConnections.set(offer.offererUserName, peerConnection);
//     //    }



//     await peerConnection.setRemoteDescription(new RTCSessionDescription(offer.offer));

//     const answer = await peerConnection.createAnswer();

//     await peerConnection.setLocalDescription(answer);

//     offer.answer = answer;
//     offer.answererUserName = userName;

//     offer.clientsInCall = clientsInCall;

//     const offerResult = await socket.emitWithAck('answerResolve2', offer);

//     offerResult.offerIceCandidates.forEach(c => {

//         if (peerConnection && peerConnection.signalingState !== 'closed')
//             peerConnection.addIceCandidate(c);
//     });

// }



async function addNewIceCandidate(iceCandidate, offer) {

    // let peerConnection = peerConnections.get( offer.answererUserName);
    // if (!peerConnection) {
    let peerConnection = await getOrCreatePeerConnection(offer.answererUserName)//createPeerConnection(offer.answererUserName);
    //     peerConnections.set(offer.offererUserName, peerConnection);
    // }


    if (iceCandidate && peerConnection && peerConnection.signalingState !== 'closed')
        peerConnection.addIceCandidate(new RTCIceCandidate(iceCandidate));
}

// async function addAnswer(offer) {

//     // let peerConnection = peerConnections.get( offer.answererUserName);
//     // if (!peerConnection) {
//     let peerConnection = await getOrCreatePeerConnection(offer.answererUserName);//createPeerConnection(offer.answererUserName);
//     //     peerConnections.set(offer.offererUserName, peerConnection);
//     // }

//     await peerConnection.setRemoteDescription(new RTCSessionDescription(offer.answer));
//     // connectedPeers.add(offer.offererUserName);
//     // connectedPeers.add(offer.answererUserName);
// }

async function getOrCreatePeerConnection(name, didIOffer = false, overide = false) {

    console.log(`getOrCreatePeerConnection : ${name}`);

    

    let peerConnection;

    //syncing more than 2 users per answered client required overwriting the peer connection.
    //need to look for a better approach in the future
   // if (!overide) {

        peerConnection = peerConnections.get(name);

        if (!peerConnection) {
            peerConnection = await createPeerConnection(name, didIOffer);
            peerConnections.set(name, peerConnection);

            await fetchUserMedia(peerConnection);

            attachIceCandidateListener(peerConnection, name, didIOffer);
            listenAndSetupRemoteVideoStream(peerConnection, name);
   

            
           
    
   
      
        }

    // } else {

    //     // peerConnection = await createPeerConnection(name, didIOffer);
    //     // peerConnections.set(name, peerConnection);


    //  //   await fetchUserMedia();
    
    //     // peerConnections.forEach((peerConnection, peerId) => {
    //     //     // Remove all tracks from the peer connection
    //     //     peerConnection.getSenders().forEach(sender => peerConnection.removeTrack(sender));

    //     //     // Add the new tracks to the peer connection
    //     //     localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));
    //     // });
  
    //    // listenAndSetupRemoteVideoStream(peerConnection, name);
    // }

   

      //  localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));

        // Assume peerConnections is a Map of RTCPeerConnection objects


  // localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));




    return peerConnection;

}

function sendLocalFeedToRemotePeers(){

      peerConnections.forEach((peerConnection, peerId) => {
            // Remove all tracks from the peer connection
            peerConnection.getSenders().forEach(sender => peerConnection.removeTrack(sender));

            // Add the new tracks to the peer connection
            localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));
        });

          // localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));


}


function listenAndSetupRemoteVideoStream(peerConnection, name){
 
    // Handle remote track
 peerConnection.ontrack = event => {
    const remoteStream = new MediaStream();
    event.streams[0].getTracks().forEach(track => remoteStream.addTrack(track));
    //console.log(`Remote stream added for ${name} : ${remoteStream}`);

    // Update or create remote video element for the target user
    const videoElement = updateRemoteVideoElement(name, remoteStream);
    videoElement.srcObject = remoteStream;

    console.log(`Remote stream added for ${name}: "elementname: ${videoElement.id} : ${remoteStream}`);

    // Play the video element when it's ready
    videoElement.onloadedmetadata = () => {
        console.log(`onloadedmetadata: Video is now ready to play for ${name}`);
        playVideoSafely(videoElement); // Function to safely play the video
    };

};

}
function playVideoSafely(videoElement) {
    videoElement.play().then(() => {
        console.log("Playback started successfully.");
    }).catch(error => {
        console.error("Error attempting to play video:", error);
        // Handle autoplay restrictions or other playback issues here
        // For example, you might show a UI element to let users manually start playback
    });
}

async function createPeerConnection(targetUserName, didIOffer = false) {
    let peerConnection = new RTCPeerConnection(peerConfiguration);


    peerConnection.oniceconnectionstatechange = () => {
        checkAndAddPeer(targetUserName, peerConnection);

        if (peerConnection.connectionState === 'disconnected' || peerConnection.connectionState === 'failed') {
            // Handle disconnection or failure
            if (connectedPeers.has(targetUserName)) {
                connectedPeers.delete(targetUserName);
                console.log(`Peer ${targetUserName} disconnected or failed to connect.`);
            }
        }
    };
    
    peerConnection.onconnectionstatechange = () => {
        checkAndAddPeer(targetUserName, peerConnection);
    };
    
    peerConnection.onsignalingstatechange = () => {
        checkAndAddPeer(targetUserName, peerConnection);
    };
    // if(!localStream)
    // await fetchUserMedia();

    // ICE connection state changes
    // peerConnection.oniceconnectionstatechange = () => {
    //     console.log(`ICE connection state changed to: ${peerConnection.iceConnectionState}`);

    //     if (//peerConnection.iceConnectionState === 'connected' ||
    //     peerConnection.iceConnectionState === 'completed') {
    //     console.log(`Connection established with ${targetUserName}`);
    //     // Ensure the peer is marked as connected
    //     connectedPeers.add(targetUserName);
    //     }
    //     else if (peerConnection.connectionState === 'disconnected' || peerConnection.connectionState === 'failed') {
    //         // Handle disconnection or failure
    //         if (connectedPeers.has(targetUserName)) {
    //             connectedPeers.delete(targetUserName);
    //             console.log(`Peer ${targetUserName} disconnected or failed to connect.`);
    //         }
    //     }


    // };

    // // Signaling state changes
    // peerConnection.onsignalingstatechange = () => {
    //     console.log(`Signaling state changed to: ${peerConnection.signalingState}`);
    // };

    // // Connection state changes
    // peerConnection.onconnectionstatechange = () => {
    //     console.log(`Connection state changed to: ${peerConnection.connectionState}`);
    // };

    // // ICE gathering state changes
    // peerConnection.onicegatheringstatechange = () => {
    //     console.log(`ICE gathering state changed to: ${peerConnection.iceGatheringState}`);
    // };






    return peerConnection;
}

function checkAndAddPeer(peerId, peerConnection) {
    if (peerConnection.iceConnectionState === 'connected' || peerConnection.iceConnectionState === 'completed') {
        if (peerConnection.connectionState === 'connected') {
            if (peerConnection.signalingState === 'stable') {

               let videoEl = videoElementsMap.get(peerId);

                // Check if the video is playing and has enough data
                if (videoEl && !videoEl.paused && videoEl.readyState === 4) {
                    // All conditions are met, add the peer to the set
                    connectedPeers.add(peerId);
                    console.log(`Peer ${peerId} added to connected peers.`);
                }
            }
        }
    }
}

//let videoIndex = 0;
// Function to update or create a new remote video element for a given userName
function updateRemoteVideoElement(userName, stream) {

    let videoEl = document.getElementById(`remoteVideo_${userName}`);
    if (!videoEl) {
        console.log(`updateRemoteVideoElement : ` + "remoteVideo_" + userName);
        videoEl = document.createElement('video');
        videoEl.id = `remoteVideo_${userName}`;
        videoEl.autoplay = true;
        videoEl.playsInline = true;

        // Create a new div element for the label
        let labelEl = document.createElement('div');
        labelEl.innerText = userName;

        // Create a new div element for the container
        let containerEl = document.createElement('div');
        containerEl.style.display = 'flex';
        containerEl.style.flexDirection = 'column';
        containerEl.style.alignItems = 'center';
        containerEl.appendChild(videoEl);
        containerEl.appendChild(labelEl);

        // Append the container element to the body
        document.body.appendChild(containerEl);



        // Set CSS properties to make the video element hidden and non-interactive
        //videoEl.style.position = 'absolute';
        // videoEl.style.visibility = 'hidden';
        videoEl.style.pointerEvents = 'none';



      //  document.body.appendChild(videoEl); // Or append to a specific container

        videoElements.push(videoEl);

        videoElementsMap.set(userName, videoEl);
        // videoIndex++;
    }
    //  videoEl.srcObject = stream;

    return videoEl;

    // if (window && window.gameInstance && window.socketIOAdapterName)


}
function attachIceCandidateListener(peerConnection, name, didIOffer) {
    peerConnection.onicecandidate = e => {
        // peerConnection.addEventListener('icecandidate', e => {
        //  console.log('ICE candidate found:', e);
        if (e.candidate) {
            socket.emit('sendIceCandidateToSignalingServer', {
                iceCandidate: e.candidate,
                iceUserName: userName,
                didIOffer,
            });
        }
    };
}


function getRemoteVideoElement(peerId) {
    let videoEl = document.getElementById(`remoteVideo_${peerId}`);
    if (!videoEl) {
        videoEl = document.createElement('video');
        videoEl.id = `remoteVideo_${peerId}`;
        videoEl.autoplay = true;
        videoEl.playsInline = true; // Add playsInline for compatibility with mobile browsers
        document.body.appendChild(videoEl); // Append to the body or a specific container
    }
    return videoEl;
}




// Flag to indicate screen sharing state
async function fetchUserMedia(peerConnection, includeVideo = true) {

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

    localStream.getTracks().forEach(track => {
        console.log(`ADDED TRACK: ${track.enabled}`);
        peerConnection.addTrack(track, localStream)
    }
    
    );
  //  originalStream.getTracks().forEach(track => peerConnection.addTrack(track, originalStream));
//    console.log("NUMBER OF LOCALTRACKS: " + localStream.getTracks().length);

    //}
}

async function call(sendToUserName, isForClientSync = false, clientToSync = null) {

    if(sendToUserName === userName){

        console.log(`Calling yourself: ${userName} `);
        return;
    }

    let peerConnection = await getOrCreatePeerConnection(sendToUserName, true, false);

    try {
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);


        

        let adjustedAnswer;
        switch (peerConnection.signalingState) {
            case 'stable':
                adjustedAnswer = `Calling ${sendToUserName}. The connection is stable.`;
                break;
            case 'have-local-offer':
                adjustedAnswer = `Calling ${sendToUserName}. The local peer has created an offer and set it in the local description.`;
                break;
            case 'have-remote-offer':
                adjustedAnswer = `Calling ${sendToUserName}. The remote peer has set the offer in the Remote description.`;
                break;
            case 'have-local-pranswer':
                adjustedAnswer = `Calling ${sendToUserName}. The local peer has created a provisional answer and set it in the local desciption.`;
                break;
            case 'have-remote-pranswer':
                adjustedAnswer = `Calling ${sendToUserName}. The remote peer has created a provisional answer and set it in the remote desciption.`;
                break;
            case 'closed':
                adjustedAnswer = `Calling ${sendToUserName}. The connection is closed.`;
                break;
            default:
                console.error('Unknown signaling state:', peerConnection.signalingState);
                return;
        }

        // Log the adjusted answer
        console.log(adjustedAnswer);

        // Emitting the offer to the signaling server with details about the target user
        socket.emit('newOffer', {
            offer,
            offererUserName: userName,
            answererUserName: sendToUserName,
            isForClientSync: isForClientSync
        });

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
document.getElementById('stopScreenSharingButton').addEventListener('click', () => {

    document.getElementById('stopScreenSharingButton').setAttribute('disabled', false);
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

            screenTrack.onended = stopScreenSharing;
            // screenStream.getTracks().forEach(track => {
            //     track.onended = stopScreenSharing;
            // });


            clientsInCall.forEach(async (name) => {

                let peerConnection = peerConnections.get(name);
                const sender = peerConnection.getSenders().find(s => s.track.kind === 'video');
                if (sender) {
                    await sender.replaceTrack(screenTrack);
                }

            });


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

        clientsInCall.forEach(async (name) => {
            let peerConnection = peerConnections.get(name);
            const sender = peerConnection.getSenders().find(s => s.track.kind === 'video');
            if (sender && videoTrack) {
                await sender.replaceTrack(videoTrack);
            }
            else
                console.error('Failed to find video sender or original video track.');
        });

        localVideo.srcObject = originalStream;
        localStream = originalStream; // Ensure localStream is set back to original
        console.log('Screen sharing stopped, and original video track restored.');
        isScreenSharing = false; // Reset screen sharing flag
    }
    else {
        console.error('No original stream available or not in screen sharing mode.');
    }

    // Refresh originalStream for future use
  //  await fetchUserMedia();
}

document.getElementById('toggleMicrophoneButton').addEventListener('click', MuteMicToggle);

function MuteMicToggle() {

    isMicrophoneMuted = !isMicrophoneMuted; // Toggle the state

    peerConnections.forEach(peerConnection => {
        const senders = peerConnection.getSenders();
        senders.forEach(sender => {
            if (sender.track && sender.track.kind === 'audio') {
                sender.track.enabled = isMicrophoneMuted;
            }
        });
    });
}

document.getElementById('toggleVideoButton').addEventListener('click', ShareVideoToggle);

async function ShareVideoToggle() {
    isVideoShared = !isVideoShared;

    peerConnections.forEach(peerConnection => {
        const senders = peerConnection.getSenders();
        senders.forEach(sender => {
            if (sender.track && sender.track.kind === 'video') {
                sender.track.enabled = isVideoShared;
            }
        });
    });



}
