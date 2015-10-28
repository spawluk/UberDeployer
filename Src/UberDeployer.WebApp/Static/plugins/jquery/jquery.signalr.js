/*!
 * ASP.NET SignalR JavaScript Library v1.0.1
 * http://signalr.net/
 *
 * Copyright Microsoft Open Technologies, Inc. All rights reserved.
 * Licensed under the Apache 2.0
 * https://github.com/SignalR/SignalR/blob/master/LICENSE.md
 *
 */
(function(n,t){"use strict";function l(t,r){var u,f;if(n.isArray(t)){for(u=t.length-1;u>=0;u--)f=t[u],n.type(t)==="object"||n.type(f)==="string"&&i.transports[f]||(r.log("Invalid transport: "+f+", removing it from the transports list."),t.splice(u,1));t.length===0&&(r.log("No transports remain within the specified transport array."),t=null)}else n.type(t)==="object"||i.transports[t]||t==="auto"||(r.log("Invalid transport: "+t.toString()),t=null);return t}function h(n){return n==="http:"?80:n==="https:"?443:void 0}function o(n,t){return t.match(/:\d+$/)?t:t+":"+h(n)}if(typeof n!="function")throw new Error("SignalR: jQuery not found. Please ensure jQuery is referenced before the SignalR.js file.");if(!t.JSON)throw new Error("SignalR: No JSON parser found. Please ensure json2.js is referenced before the SignalR.js file if you need to support clients without native JSON parsing support, e.g. IE<8.");var i,s,e=t.document.readyState==="complete",f=n(t),r={onStart:"onStart",onStarting:"onStarting",onReceived:"onReceived",onError:"onError",onConnectionSlow:"onConnectionSlow",onReconnecting:"onReconnecting",onReconnect:"onReconnect",onStateChanged:"onStateChanged",onDisconnect:"onDisconnect"},c=function(n,i){if(i!==!1){var r;typeof t.console!="undefined"&&(r="["+(new Date).toTimeString()+"] SignalR: "+n,t.console.debug?t.console.debug(r):t.console.log&&t.console.log(r))}},u=function(t,i,u){return i===t.state?(t.state=u,n(t).triggerHandler(r.onStateChanged,[{oldState:i,newState:u}]),!0):!1},a=function(n){return n.state===i.connectionState.disconnected},v=function(n){var u,r;n._.configuredStopReconnectingTimeout||(r=function(n){n.log("Couldn't reconnect within the configured timeout ("+n.disconnectTimeout+"ms), disconnecting."),n.stop(!1,!1)},n.reconnecting(function(){var n=this;n.state===i.connectionState.reconnecting&&(u=t.setTimeout(function(){r(n)},n.disconnectTimeout))}),n.stateChanged(function(n){n.oldState===i.connectionState.reconnecting&&t.clearTimeout(u)}),n._.configuredStopReconnectingTimeout=!0)};i=function(n,t,r){return new i.fn.init(n,t,r)},i.events=r,i.changeState=u,i.isDisconnecting=a,i.connectionState={connecting:0,connected:1,reconnecting:2,disconnected:4},i.hub={start:function(){throw new Error("SignalR: Error loading hubs. Ensure your hubs reference is correct, e.g. <script src='/signalr/hubs'><\/script>.");}},f.load(function(){e=!0}),i.fn=i.prototype={init:function(n,t,i){this.url=n,this.qs=t,this._={},typeof i=="boolean"&&(this.logging=i)},isCrossDomain:function(i,r){var u;return(i=n.trim(i),i.indexOf("http")!==0)?!1:(r=r||t.location,u=t.document.createElement("a"),u.href=i,u.protocol+o(u.protocol,u.host)!==r.protocol+o(r.protocol,r.host))},ajaxDataType:"json",logging:!1,state:i.connectionState.disconnected,keepAliveData:{},reconnectDelay:2e3,disconnectTimeout:3e4,keepAliveWarnAt:2/3,start:function(o,s){var h=this,c={waitForPageLoad:!0,transport:"auto",jsonp:!1},w,a=h._deferral||n.Deferred(),y=t.document.createElement("a"),p;if(n.type(o)==="function"?s=o:n.type(o)==="object"&&(n.extend(c,o),n.type(c.callback)==="function"&&(s=c.callback)),c.transport=l(c.transport,h),!c.transport)throw new Error("SignalR: Invalid transport(s) specified, aborting start.");return!e&&c.waitForPageLoad===!0?(f.load(function(){h._deferral=a,h.start(o,s)}),a.promise()):(v(h),u(h,i.connectionState.disconnected,i.connectionState.connecting)===!1)?(a.resolve(h),a.promise()):(y.href=h.url,y.protocol&&y.protocol!==":"?(h.protocol=y.protocol,h.host=y.host,h.baseUrl=y.protocol+"//"+y.host):(h.protocol=t.document.location.protocol,h.host=t.document.location.host,h.baseUrl=h.protocol+"//"+h.host),h.wsProtocol=h.protocol==="https:"?"wss://":"ws://",c.transport==="auto"&&c.jsonp===!0&&(c.transport="longPolling"),this.isCrossDomain(h.url)&&(h.log("Auto detected cross domain url."),c.transport==="auto"&&(c.transport=["webSockets","longPolling"]),c.jsonp||(c.jsonp=!n.support.cors,c.jsonp&&h.log("Using jsonp because this browser doesn't support CORS"))),h.ajaxDataType=c.jsonp?"jsonp":"json",n(h).bind(r.onStart,function(){n.type(s)==="function"&&s.call(h),a.resolve(h)}),w=function(t,e){if(e=e||0,e>=t.length){h.transport||(n(h).triggerHandler(r.onError,"SignalR: No transport could be initialized successfully. Try specifying a different transport or none at all for auto initialization."),a.reject("SignalR: No transport could be initialized successfully. Try specifying a different transport or none at all for auto initialization."),h.stop());return}var o=t[e],s=n.type(o)==="object"?o:i.transports[o];if(o.indexOf("_")===0){w(t,e+1);return}s.start(h,function(){s.supportsKeepAlive&&h.keepAliveData.activated&&i.transports._logic.monitorKeepAlive(h),h.transport=s,u(h,i.connectionState.connecting,i.connectionState.connected),n(h).triggerHandler(r.onStart),f.unload(function(){h.stop(!1)})},function(){w(t,e+1)})},p=h.url+"/negotiate",p=i.transports._logic.addQs(p,h),h.log("Negotiating with '"+p+"'."),n.ajax({url:p,global:!1,cache:!1,type:"GET",data:{},dataType:h.ajaxDataType,error:function(t){n(h).triggerHandler(r.onError,[t.responseText]),a.reject("SignalR: Error during negotiation request: "+t.responseText),h.stop()},success:function(t){var u=h.keepAliveData,e,f;if(h.appRelativeUrl=t.Url,h.id=t.ConnectionId,h.token=t.ConnectionToken,h.webSocketServerUrl=t.WebSocketServerUrl,h.disconnectTimeout=t.DisconnectTimeout*1e3,t.KeepAliveTimeout?(u.activated=!0,u.timeout=t.KeepAliveTimeout*1e3,u.timeoutWarning=u.timeout*h.keepAliveWarnAt,u.checkInterval=(u.timeout-u.timeoutWarning)/3):u.activated=!1,!t.ProtocolVersion||t.ProtocolVersion!=="1.2"){n(h).triggerHandler(r.onError,"You are using a version of the client that isn't compatible with the server. Client version 1.2, server version "+t.ProtocolVersion+"."),a.reject("You are using a version of the client that isn't compatible with the server. Client version 1.2, server version "+t.ProtocolVersion+".");return}n(h).triggerHandler(r.onStarting),e=[],f=[],n.each(i.transports,function(n){if(n==="webSockets"&&!t.TryWebSockets)return!0;f.push(n)}),n.isArray(c.transport)?n.each(c.transport,function(){var t=this;(n.type(t)==="object"||n.type(t)==="string"&&n.inArray(""+t,f)>=0)&&e.push(n.type(t)==="string"?""+t:t)}):n.type(c.transport)==="object"||n.inArray(c.transport,f)>=0?e.push(c.transport):e=f,w(e)}}),a.promise())},starting:function(t){var i=this;return n(i).bind(r.onStarting,function(){t.call(i)}),i},send:function(n){var t=this;if(t.state===i.connectionState.disconnected)throw new Error("SignalR: Connection must be started before data can be sent. Call .start() before .send()");if(t.state===i.connectionState.connecting)throw new Error("SignalR: Connection has not been fully initialized. Use .start().done() or .start().fail() to run logic after the connection has started.");return t.transport.send(t,n),t},received:function(t){var i=this;return n(i).bind(r.onReceived,function(n,r){t.call(i,r)}),i},stateChanged:function(t){var i=this;return n(i).bind(r.onStateChanged,function(n,r){t.call(i,r)}),i},error:function(t){var i=this;return n(i).bind(r.onError,function(n,r){t.call(i,r)}),i},disconnected:function(t){var i=this;return n(i).bind(r.onDisconnect,function(){t.call(i)}),i},connectionSlow:function(t){var i=this;return n(i).bind(r.onConnectionSlow,function(){t.call(i)}),i},reconnecting:function(t){var i=this;return n(i).bind(r.onReconnecting,function(){t.call(i)}),i},reconnected:function(t){var i=this;return n(i).bind(r.onReconnect,function(){t.call(i)}),i},stop:function(t,f){var e=this;if(e.state!==i.connectionState.disconnected){try{e.transport&&(f!==!1&&e.transport.abort(e,t),e.transport.supportsKeepAlive&&e.keepAliveData.activated&&i.transports._logic.stopMonitoringKeepAlive(e),e.transport.stop(e),e.transport=null),n(e).triggerHandler(r.onDisconnect),delete e.messageId,delete e.groupsToken,delete e.id,delete e._deferral}finally{u(e,e.state,i.connectionState.disconnected)}return e}},log:function(n){c(n,this.logging)}},i.fn.init.prototype=i.fn,i.noConflict=function(){return n.connection===i&&(n.connection=s),i},n.connection&&(s=n.connection),n.connection=n.signalR=i})(window.jQuery,window),function(n,t){"use strict";function u(f){var e=f.keepAliveData,o,s;f.state===r.connectionState.connected&&(o=new Date,o.setTime(o-e.lastKeepAlive),s=o.getTime(),s>=e.timeout?(f.log("Keep alive timed out.  Notifying transport that connection has been lost."),f.transport.lostConnection(f)):s>=e.timeoutWarning?e.userNotified||(f.log("Keep alive has been missed, connection may be dead/slow."),n(f).triggerHandler(i.onConnectionSlow),e.userNotified=!0):e.userNotified=!1),e.monitoring&&t.setTimeout(function(){u(f)},e.checkInterval)}var r=n.signalR,i=n.signalR.events,f=n.signalR.changeState;r.transports={},r.transports._logic={pingServer:function(t,i){var f=i==="webSockets"?"":t.baseUrl,u=f+t.appRelativeUrl+"/ping",r=n.Deferred();return u=this.addQs(u,t),n.ajax({url:u,global:!1,cache:!1,type:"GET",data:{},dataType:t.ajaxDataType,success:function(n){n.Response==="pong"?r.resolve():r.reject("SignalR: Invalid ping response when pinging server: "+(n.responseText||n.statusText))},error:function(n){r.reject("SignalR: Error pinging server: "+(n.responseText||n.statusText))}}),r.promise()},addQs:function(t,i){var u=t.indexOf("?")!==-1?"&":"?",r;if(!i.qs)return t;if(typeof i.qs=="object")return t+u+n.param(i.qs);if(typeof i.qs=="string")return r=i.qs.charAt(0),(r==="?"||r==="&")&&(u=""),t+u+i.qs;throw new Error("Connections query string property must be either a string or object.");},getUrl:function(n,i,r,u){var o=i==="webSockets"?"":n.baseUrl,f=o+n.appRelativeUrl,e="transport="+i+"&connectionToken="+t.encodeURIComponent(n.token);return n.data&&(e+="&connectionData="+t.encodeURIComponent(n.data)),n.groupsToken&&(e+="&groupsToken="+t.encodeURIComponent(n.groupsToken)),r?(u&&(f=f+"/reconnect"),n.messageId&&(e+="&messageId="+t.encodeURIComponent(n.messageId))):f=f+"/connect",f+="?"+e,f=this.addQs(f,n),f+="&tid="+Math.floor(Math.random()*11)},maximizePersistentResponse:function(n){return{MessageId:n.C,Messages:n.M,Disconnect:typeof n.D!="undefined"?!0:!1,TimedOut:typeof n.T!="undefined"?!0:!1,LongPollDelay:n.L,GroupsToken:n.G}},updateGroups:function(n,t){t&&(n.groupsToken=t)},ajaxSend:function(r,u){var f=r.url+"/send?transport="+r.transport.name+"&connectionToken="+t.encodeURIComponent(r.token);return f=this.addQs(f,r),n.ajax({url:f,global:!1,type:r.ajaxDataType==="jsonp"?"GET":"POST",dataType:r.ajaxDataType,data:{data:u},success:function(t){t&&n(r).triggerHandler(i.onReceived,[t])},error:function(t,u){u!=="abort"&&u!=="parsererror"&&n(r).triggerHandler(i.onError,[t])}})},ajaxAbort:function(i,r){if(typeof i.transport!="undefined"){r=typeof r=="undefined"?!0:r;var u=i.url+"/abort?transport="+i.transport.name+"&connectionToken="+t.encodeURIComponent(i.token);u=this.addQs(u,i),n.ajax({url:u,async:r,timeout:1e3,global:!1,type:"POST",dataType:i.ajaxDataType,data:{}}),i.log("Fired ajax abort async = "+r)}},processMessages:function(t,r){var u,f;if(t.transport){if(f=n(t),t.transport.supportsKeepAlive&&t.keepAliveData.activated&&this.updateKeepAlive(t),!r)return;if(u=this.maximizePersistentResponse(r),u.Disconnect){t.log("Disconnect command received from server"),t.stop(!1,!1);return}this.updateGroups(t,u.GroupsToken),u.Messages&&n.each(u.Messages,function(){f.triggerHandler(i.onReceived,[this])}),u.MessageId&&(t.messageId=u.MessageId)}},monitorKeepAlive:function(t){var r=t.keepAliveData,f=this;r.monitoring?t.log("Tried to monitor keep alive but it's already being monitored"):(r.monitoring=!0,f.updateKeepAlive(t),t.keepAliveData.reconnectKeepAliveUpdate=function(){f.updateKeepAlive(t)},n(t).bind(i.onReconnect,t.keepAliveData.reconnectKeepAliveUpdate),t.log("Now monitoring keep alive with a warning timeout of "+r.timeoutWarning+" and a connection lost timeout of "+r.timeout),u(t))},stopMonitoringKeepAlive:function(t){var r=t.keepAliveData;r.monitoring&&(r.monitoring=!1,n(t).unbind(i.onReconnect,t.keepAliveData.reconnectKeepAliveUpdate),t.keepAliveData={},t.log("Stopping the monitoring of the keep alive"))},updateKeepAlive:function(n){n.keepAliveData.lastKeepAlive=new Date},ensureReconnectingState:function(t){return f(t,r.connectionState.connected,r.connectionState.reconnecting)===!0&&n(t).triggerHandler(i.onReconnecting),t.state===r.connectionState.reconnecting},foreverFrame:{count:0,connections:{}}}}(window.jQuery,window),function(n,t){"use strict";var i=n.signalR,u=n.signalR.events,f=n.signalR.changeState,r=i.transports._logic;i.transports.webSockets={name:"webSockets",supportsKeepAlive:!0,attemptingReconnect:!1,currentSocketID:0,send:function(n,t){n.socket.send(t)},start:function(e,o,s){var c,a=!1,h=this,l=!o,v=n(e);if(!t.WebSocket){s();return}e.socket||(c=e.webSocketServerUrl?e.webSocketServerUrl:e.wsProtocol+e.host,c+=r.getUrl(e,this.name,l),e.log("Connecting to websocket endpoint '"+c+"'"),e.socket=new t.WebSocket(c),e.socket.ID=++h.currentSocketID,e.socket.onopen=function(){a=!0,e.log("Websocket opened"),h.attemptingReconnect&&(h.attemptingReconnect=!1),o?o():f(e,i.connectionState.reconnecting,i.connectionState.connected)===!0&&v.triggerHandler(u.onReconnect)},e.socket.onclose=function(t){if(this.ID===h.currentSocketID){if(a)typeof t.wasClean!="undefined"&&t.wasClean===!1?(n(e).triggerHandler(u.onError,[t.reason]),e.log("Unclean disconnect from websocket."+t.reason)):e.log("Websocket closed");else{s?s():l&&h.reconnect(e);return}h.reconnect(e)}},e.socket.onmessage=function(i){var f=t.JSON.parse(i.data),o=n(e);f&&(n.isEmptyObject(f)||f.M?r.processMessages(e,f):o.triggerHandler(u.onReceived,[f]))})},reconnect:function(n){var u=this;n.state!==i.connectionState.disconnected&&(u.attemptingReconnect||(u.attemptingReconnect=!0),t.setTimeout(function(){u.attemptingReconnect&&u.stop(n),r.ensureReconnectingState(n)&&(n.log("Websocket reconnecting"),u.start(n))},n.reconnectDelay))},lostConnection:function(n){this.reconnect(n)},stop:function(n){n.socket!==null&&(n.log("Closing the Websocket"),n.socket.close(),n.socket=null)},abort:function(){}}}(window.jQuery,window),function(n,t){"use strict";var r=n.signalR,u=n.signalR.events,f=n.signalR.changeState,i=r.transports._logic;r.transports.serverSentEvents={name:"serverSentEvents",supportsKeepAlive:!0,reconnectTimeout:!1,currentEventSourceID:0,timeOut:3e3,start:function(e,o,s){var h=this,l=!1,y=n(e),c=!o,v,a;if(e.eventSource&&(e.log("The connection already has an event source. Stopping it."),e.stop()),!t.EventSource){s&&(e.log("This browser doesn't support SSE."),s());return}v=i.getUrl(e,this.name,c);try{e.log("Attempting to connect to SSE endpoint '"+v+"'"),e.eventSource=new t.EventSource(v),e.eventSource.ID=++h.currentEventSourceID}catch(p){e.log("EventSource failed trying to connect with error "+p.Message),s?s():(y.triggerHandler(u.onError,[p]),c&&h.reconnect(e));return}a=t.setTimeout(function(){l===!1&&(e.log("EventSource timed out trying to connect"),e.log("EventSource readyState: "+e.eventSource.readyState),c||h.stop(e),c?e.eventSource.readyState!==t.EventSource.CONNECTING&&e.eventSource.readyState!==t.EventSource.OPEN&&h.reconnect(e):s&&s())},h.timeOut),e.eventSource.addEventListener("open",function(){e.log("EventSource connected"),a&&t.clearTimeout(a),h.reconnectTimeout&&t.clearTimeout(h.reconnectTimeout),l===!1&&(l=!0,o?o():f(e,r.connectionState.reconnecting,r.connectionState.connected)===!0&&y.triggerHandler(u.onReconnect))},!1),e.eventSource.addEventListener("message",function(n){n.data!=="initialized"&&i.processMessages(e,t.JSON.parse(n.data))},!1),e.eventSource.addEventListener("error",function(n){if(this.ID===h.currentEventSourceID){if(!l){s&&s();return}e.log("EventSource readyState: "+e.eventSource.readyState),n.eventPhase===t.EventSource.CLOSED?(e.log("EventSource reconnecting due to the server connection ending"),h.reconnect(e)):(e.log("EventSource error"),y.triggerHandler(u.onError))}},!1)},reconnect:function(n){var r=this;r.reconnectTimeout=t.setTimeout(function(){r.stop(n),i.ensureReconnectingState(n)&&(n.log("EventSource reconnecting"),r.start(n))},n.reconnectDelay)},lostConnection:function(n){this.reconnect(n)},send:function(n,t){i.ajaxSend(n,t)},stop:function(n){n&&n.eventSource&&(n.log("EventSource calling close()"),n.eventSource.ID=null,n.eventSource.close(),n.eventSource=null,delete n.eventSource)},abort:function(n,t){i.ajaxAbort(n,t)}}}(window.jQuery,window),function(n,t){"use strict";var r=n.signalR,u=n.signalR.events,f=n.signalR.changeState,i=r.transports._logic;r.transports.foreverFrame={name:"foreverFrame",supportsKeepAlive:!0,timeOut:3e3,start:function(r,u,f){var o=this,s=i.foreverFrame.count+=1,h,e=n("<iframe data-signalr-connection-id='"+r.id+"' style='position:absolute;top:0;left:0;width:0;height:0;visibility:hidden;' src=''></iframe>");if(t.EventSource){f&&(r.log("This browser supports SSE, skipping Forever Frame."),f());return}h=i.getUrl(r,this.name),h+="&frameId="+s,n("body").append(e),e.prop("src",h),i.foreverFrame.connections[s]=r,r.log("Binding to iframe's readystatechange event."),e.bind("readystatechange",function(){n.inArray(this.readyState,["loaded","complete"])>=0&&(r.log("Forever frame iframe readyState changed to "+this.readyState+", reconnecting"),o.reconnect(r))}),r.frame=e[0],r.frameId=s,u&&(r.onSuccess=u),t.setTimeout(function(){r.onSuccess&&(r.log("Failed to connect using forever frame source, it timed out after "+o.timeOut+"ms."),o.stop(r),f&&f())},o.timeOut)},reconnect:function(n){var r=this;t.setTimeout(function(){if(n.frame&&i.ensureReconnectingState(n)){var u=n.frame,t=i.getUrl(n,r.name,!0)+"&frameId="+n.frameId;n.log("Updating iframe src to '"+t+"'."),u.src=t}},n.reconnectDelay)},lostConnection:function(n){this.reconnect(n)},send:function(n,t){i.ajaxSend(n,t)},receive:function(t,r){var u;i.processMessages(t,r),t.frameMessageCount=(t.frameMessageCount||0)+1,t.frameMessageCount>50&&(t.frameMessageCount=0,u=t.frame.contentWindow||t.frame.contentDocument,u&&u.document&&n("body",u.document).empty())},stop:function(t){var r=null;if(t.frame){if(t.frame.stop)t.frame.stop();else try{r=t.frame.contentWindow||t.frame.contentDocument,r.document&&r.document.execCommand&&r.document.execCommand("Stop")}catch(u){t.log("SignalR: Error occured when stopping foreverFrame transport. Message = "+u.message)}n(t.frame).remove(),delete i.foreverFrame.connections[t.frameId],t.frame=null,t.frameId=null,delete t.frame,delete t.frameId,t.log("Stopping forever frame")}},abort:function(n,t){i.ajaxAbort(n,t)},getConnection:function(n){return i.foreverFrame.connections[n]},started:function(t){t.onSuccess?(t.onSuccess(),t.onSuccess=null,delete t.onSuccess):f(t,r.connectionState.reconnecting,r.connectionState.connected)===!0&&n(t).triggerHandler(u.onReconnect)}}}(window.jQuery,window),function(n,t){"use strict";var r=n.signalR,f=n.signalR.events,e=n.signalR.changeState,u=n.signalR.isDisconnecting,i=r.transports._logic;r.transports.longPolling={name:"longPolling",supportsKeepAlive:!1,reconnectDelay:3e3,init:function(n,r){var e=this,f,o=function(i){u(n)===!1&&(n.log("SignalR: Server ping failed because '"+i+"', re-trying ping."),t.setTimeout(f,e.reconnectDelay))};n.log("SignalR: Initializing long polling connection with server."),f=function(){i.pingServer(n,e.name).done(r).fail(o)},f()},start:function(o,s){var c=this,a=!1,l=function(){a||(a=!0,s(),o.log("Longpolling connected"))};o.pollXhr&&(o.log("Polling xhr requests already exists, aborting."),o.stop()),c.init(o,function(){o.messageId=null,t.setTimeout(function(){(function s(h,a){var p=h.messageId,w=p===null,v=!w,y=i.getUrl(h,c.name,v,a);u(h)!==!0&&(o.log("Attempting to connect to '"+y+"' using longPolling."),h.pollXhr=n.ajax({url:y,global:!1,cache:!1,type:"GET",dataType:o.ajaxDataType,success:function(r){var e=0,f;(l(),r&&(f=i.maximizePersistentResponse(r)),i.processMessages(h,r),f&&n.type(f.LongPollDelay)==="number"&&(e=f.LongPollDelay),f&&f.Disconnect)||u(h)!==!0&&(e>0?t.setTimeout(function(){s(h,!1)},e):s(h,!1))},error:function(t,u){if(u==="abort"){o.log("Aborted xhr requst.");return}o.state!==r.connectionState.reconnecting&&(o.log("An error occurred using longPolling. Status = "+u+". "+t.responseText),n(h).triggerHandler(f.onError,[t.responseText])),i.ensureReconnectingState(h),c.init(h,function(){s(h,!0)})}}),v&&a===!0&&e(o,r.connectionState.reconnecting,r.connectionState.connected)===!0&&(o.log("Raising the reconnect event"),n(h).triggerHandler(f.onReconnect)))})(o),t.setTimeout(function(){l()},250)},250)})},lostConnection:function(){throw new Error("Lost Connection not handled for LongPolling");},send:function(n,t){i.ajaxSend(n,t)},stop:function(n){n.pollXhr&&(n.pollXhr.abort(),n.pollXhr=null,delete n.pollXhr)},abort:function(n,t){i.ajaxAbort(n,t)}}}(window.jQuery,window),function(n,t){"use strict";function f(n){return n+s}function h(t){return n.isFunction(t)?null:n.type(t)==="undefined"?null:t}function o(n){for(var t in n)if(n.hasOwnProperty(t))return!0;return!1}function r(n,t){return new r.fn.init(n,t)}function i(t,r){var u={qs:null,logging:!1,useDefaultPath:!0};return n.extend(u,r),(!t||u.useDefaultPath)&&(t=(t||"")+"/signalr"),new i.fn.init(t,u)}var e=0,u={},s=".hubProxy";Array.prototype.hasOwnProperty("map")||(Array.prototype.map=function(n,t){for(var r=this,f=r.length,u=[],i=0;i<f;i+=1)r.hasOwnProperty(i)&&(u[i]=n.call(t,r[i],i,r));return u}),r.fn=r.prototype={init:function(n,t){this.state={},this.connection=n,this.hubName=t,this._={callbackMap:{}}},hasSubscriptions:function(){return o(this._.callbackMap)},on:function(t,i){var u=this,r=u._.callbackMap;return t=t.toLowerCase(),r[t]||(r[t]={}),r[t][i]=function(n,t){i.apply(u,t)},n(u).bind(f(t),r[t][i]),u},off:function(t,i){var u=this,e=u._.callbackMap,r;return t=t.toLowerCase(),r=e[t],r&&(r[i]?(n(u).unbind(f(t),r[i]),delete r[i],o(r)||delete e[t]):i||(n(u).unbind(f(t)),delete e[t])),u},invoke:function(i){var r=this,c=n.makeArray(arguments).slice(1),l=c.map(h),o={H:r.hubName,M:i,A:l,I:e},f=n.Deferred(),s=function(t){var i=r._maximizeHubResponse(t);n.extend(r.state,i.State),i.Error?(i.StackTrace&&r.connection.log(i.Error+"\n"+i.StackTrace),f.rejectWith(r,[i.Error])):f.resolveWith(r,[i.Result])};return u[e.toString()]={scope:r,method:s},e+=1,n.isEmptyObject(r.state)||(o.S=r.state),r.connection.send(t.JSON.stringify(o)),f.promise()},_maximizeHubResponse:function(n){return{State:n.S,Result:n.R,Id:n.I,Error:n.E,StackTrace:n.T}}},r.fn.init.prototype=r.fn,i.fn=i.prototype=n.connection(),i.fn.init=function(t,i){var e={qs:null,logging:!1,useDefaultPath:!0},r=this;n.extend(e,i),n.signalR.fn.init.call(r,t,e.qs,e.logging),r.proxies={},r.received(function(t){var i,s,o,e,c,h;t&&(typeof t.I!="undefined"?(o=t.I.toString(),e=u[o],e&&(u[o]=null,delete u[o],e.method.call(e.scope,t))):(i=this._maximizeClientHubInvocation(t),r.log("Triggering client hub event '"+i.Method+"' on hub '"+i.Hub+"'."),c=i.Hub.toLowerCase(),h=i.Method.toLowerCase(),s=this.proxies[c],n.extend(s.state,i.State),n(s).triggerHandler(f(h),[i.Args])))})},i.fn._maximizeClientHubInvocation=function(n){return{Hub:n.H,Method:n.M,Args:n.A,State:n.S}},i.fn._registerSubscribedHubs=function(){this._subscribedToHubs||(this._subscribedToHubs=!0,this.starting(function(){var i=[];n.each(this.proxies,function(n){this.hasSubscriptions()&&i.push({name:n})}),this.data=t.JSON.stringify(i)}))},i.fn.createHubProxy=function(n){n=n.toLowerCase();var t=this.proxies[n];return t||(t=r(this,n),this.proxies[n]=t),this._registerSubscribedHubs(),t},i.fn.init.prototype=i.fn,n.hubConnection=i}(window.jQuery,window),function(n){n.signalR.version="1.0.1"}(window.jQuery)