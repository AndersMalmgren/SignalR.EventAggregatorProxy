(window.stubHub = function() {
	window.signalR = {};
	window.signalR.HubConnectionBuilder = function()
	{
		
	}
	
	window.signalR.HubConnectionBuilder.prototype =
	{
		withUrl: function() { return this},
		build: function() { 
				return window.currentHub = window.currentHub || {
				start: function() { 
					return { then: function(callback) { callback(); } }
				},
				on: function() {},
				onclose: function(callback) {
					window.currentOnClose = callback;
				}				
			}
		}
	}    
})();
