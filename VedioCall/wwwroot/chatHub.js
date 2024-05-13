
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7225/Chat-Hub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function login() {
    var username = document.getElementById("email").value;
    var password = document.getElementById("password").value;

    fetch("https://localhost:7225/api/Login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ Email: username, Password: password })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
        })
        .then(async(_) =>  {
            await connection.start();
            console.log("SignalR Connected.");
            document.getElementById("login").style.display = "none";
            document.getElementById("call-section").style.display = "block";
        })
        .catch(error => {
            console.error('Error:', error);
        });
}
let peerConnection;

async function startCall() {
    const targetUserId = document.getElementById("users-dropdown").value;
    const offerOptions = {
        offerToReceiveAudio: 1,
        offerToReceiveVideo: 1
    };

    try {
        peerConnection = new RTCPeerConnection();
        peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                sendIceCandidate(targetUserId, event.candidate);
            }
        };

        const offer = await peerConnection.createOffer(offerOptions);
        await peerConnection.setLocalDescription(offer);

        await connection.invoke("StartCall", targetUserId, offer.sdp);
    } catch (err) {
        console.error(err);
    }
}
async function answerCall(callerId, sdpOffer) {
    const answerOptions = {
        offerToReceiveAudio: 1,
        offerToReceiveVideo: 1
    };

    try {
        peerConnection = new RTCPeerConnection();
        peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                sendIceCandidate(callerId, event.candidate);
            }
        };

        await peerConnection.setRemoteDescription({ type: "offer", sdp: sdpOffer });
        const answer = await peerConnection.createAnswer(answerOptions);
        await peerConnection.setLocalDescription(answer);

        await connection.invoke("AnswerCall", callerId, answer.sdp);
    } catch (err) {
        console.error(err);
    }
}

async function rejectCall(callerId) {
    try {
        await connection.invoke("RejectCall", callerId);
    } catch (err) {
        console.error(err);
    }
}
async function sendIceCandidate(targetUserId, candidate) {
    try {
        await connection.invoke("SendIceCandidate", targetUserId, candidate);
    } catch (err) {
        console.error(err);
    }
}

async function acceptCall(callerName) {
    try {
        await hubConnection.invoke("AcceptCall", callerName);
    } catch (error) {
        console.error(error);
    }
}

async function rejectCall(callerName) {
    try {
        await hubConnection.invoke("RejectCall", callerName);
    } catch (error) {
        console.error(error);
    }
}

/*connectedUsersDropdown.addEventListener("change", () => {
    calleeNameSelect.value = connectedUsersDropdown.value;
});*/

async function answerCall(callerId, sdpOffer) {
    try {
        await connection.invoke("AnswerCall", callerId, /* pass SDP offer */);
    } catch (err) {
        console.error(err);
    }
}

async function rejectCall(callerId) {
    try {
        await connection.invoke("RejectCall", callerId);
    } catch (err) {
        console.error(err);
    }
}

connection.on("ReceiveIceCandidate", async (callerId, candidate) => {
    try {
        // Add the received ICE candidate to the peer connection
        await peerConnection.addIceCandidate(candidate);
    } catch (error) {
        console.error("Error adding ICE candidate:", error);
    }
});
connection.on("UserConnect", (users) => {
    const dropdown = document.getElementById("users-dropdown");
    dropdown.innerHTML = "";
    users.forEach(user => {
        const option = document.createElement("option");
        option.text = user.name;
        option.value = user.id;
        dropdown.appendChild(option);
    });
});

connection.on("Busy", () => {
    alert("The user you're trying to call is busy.");
});

connection.on("ReceiveCall", (callerId, sdpOffer) => {
    const acceptCall = confirm("Incoming call. Accept?");
    if (acceptCall) {
        answerCall(callerId);
    } else {
        rejectCall(callerId);
    }
});

connection.on("CallRejected", () => {
    alert("The call was rejected by the other user.");
});
connection.on("CallAccepted", () => {
    startMediaStream();
});

connection.on("CallEnded", () => {
    stopMediaStream();
});

async function startMediaStream() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true, video: true });
        const localVideo = document.getElementById("local-video");
        localVideo.srcObject = stream;
        localVideo.play();
    } catch (error) {
        console.error("Error starting media stream:", error);
    }
}
function stopMediaStream() {
    const localVideo = document.getElementById("local-video");
    const stream = localVideo.srcObject;
    const tracks = stream.getTracks();
    tracks.forEach(track => track.stop());
    localVideo.srcObject = null;
}