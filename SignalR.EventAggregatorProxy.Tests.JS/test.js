/// <reference path="stubs.js" />
/// <reference path="../SignalR.EventAggregatorProxy.Client.JS/signalR.eventAggregator.js"/>

constructClientEvent = function () {
    return window.TestEvent = function () { }
}

constructEvent = function (genericArgument, event, proxyEvent, typeName) {
    if (event == null) {
        event = function() {
        };
    } else {
        event = event.genericConstructor || event;
    }
    
    var constructor = event;

    constructor.type = typeName || constructor.toString();
    if (proxyEvent == null || proxyEvent === true) {
        event.proxyEvent = true;
    }

    if (genericArgument != null) {
        event = {
            genericConstructor: constructor,
            genericArguments: [genericArgument]
        };
    }

    return genericArgument ? {
        event: event,
        genericConstructor: constructor
    }  : event;
};

test = function (name, setup) {
    var runner = function (assert) {
        if (currentHub) {
            delete currentHub;
        }

        signalR.eventAggregator = new signalR.EventAggregator(true);
        setup(assert);
    };

    window.stubHub();
    QUnit.test(name, runner);
};

throttleTest = function (name, setup) {
    test(name, function (assert) {
        var done = assert.async();
        var throttleAssert = setup(assert);
        done();
        
        if (throttleAssert) {
            throttleAssert();
        }
    });
};

test("Event aggregator class should have correct casing on closure (signalR) Issue #1 ", function (assert) {
    assert.ok(window.signalR !== undefined);
    assert.ok(window.signalR.eventAggregator !== undefined);
});

throttleTest("When subscribing to client side event Issue #2", function (assert) {
    assert.expect(0);
    currentHub.invoke = function () {
        assert.ok(false, "Server side subscribe should not be called for client side events");
    };

    signalR.eventAggregator.subscribe(constructClientEvent(), function() {
    }, {});
});

test("When unsubscribing to client side event Issue #5", function (assert) {
    var context = {};
    currentHub.invoke = function () {
        assert.ok(false, "Server side unsubscribe should not be called for client side events");
    };

    signalR.eventAggregator.subscribe(constructClientEvent(), function () {
    }, context);
    signalR.eventAggregator.unsubscribe(context);
    assert.ok(true, "Server side unsubscribe was not called");
});

test("When subscribing to client side event", function (assert) {
    var context = {};
    var eventType = constructClientEvent();
    var event = new eventType();
    var failIfPublished = false;
    signalR.eventAggregator.subscribe(eventType, function (e) {
        if (failIfPublished) {
            assert.ok(false, "Listener should not have been called");
        }
        assert.equal(event, e);
    }, context);

    signalR.eventAggregator.publish(event);
    failIfPublished = true;
    signalR.eventAggregator.unsubscribe(context);
    signalR.eventAggregator.publish(event);
});

test("When unsubscribing a context that never were subscribed", function (assert) {
    var context = {};
    try {
        signalR.eventAggregator.unsubscribe(context);
    } catch (err) {
        assert.ok(false, "Should not crash");
    }

    assert.ok(true, "It should exit function call without exception");
});

throttleTest("When unsubscribing and subscribing directly after to server side events", function (assert) {
    var event = constructEvent();
    var eventData = { type: event };

    var unsubscribeDone = false;
    var doneCallback = null;

    currentHub.invoke = function (method) {
		if(method === "Unsubscribe")
			return {
				then: function (callback) {
					doneCallback = callback;
				}
			};
		if(method === "Subscribe")
			assert.ok(unsubscribeDone, "It should not call subscribe while unsubscribe is working");
		else			
			assert.ok(false, "No method mapped");
    };


    signalR.eventAggregator.proxy.unsubscribe([eventData]);
    signalR.eventAggregator.proxy.subscribe(event);

    return function() {
        unsubscribeDone = true;
        doneCallback();
		
		currentHub.invoke = null;
    };
});

throttleTest("When doing subsequent calls to subscribe Issue #12", function (assert) {
    currentHub.invoke = function (method, arr) {
        assert.equal(arr.length, 2, "Should throttle subscriptions");
    };
    
    for (var i = 0; i < 2; i++) {
        var event = constructEvent();
        
        signalR.eventAggregator.subscribe(event, function() {
        }, {});
    }
});

throttleTest("When a third party lib is traversing object tree that has reference to eventAggregator", function (assert) {
    assert.expect(0);
    var traverse = function (obj) {
        for (var member in obj) {
            var child = obj[member];

            if (child !== null && typeof child === "object") {
                traverse(obj[member]);
            }
        }
    };

    currentHub.invoke = function () {
    };

    var event = constructEvent();

    var ViewModel = function () {
        signalR.eventAggregator.subscribe(event, this.onEvent, this);
        traverse(this);
    };

    ViewModel.prototype = {
        onEvent: function () {

        }
    };

    var vm = new ViewModel();
});

multipleSameEventSubscriptionTest = function(name, genericArgument, constraint, expectedCount) {
    throttleTest(name, function (assert) {
        currentHub.invoke = function (method, s) {
            expectedCount = expectedCount || 1;
            assert.equal(s.length, expectedCount, "It should subscribe " + expectedCount + " times to same event");
        };

        var event = function () {
        };
        event.proxyEvent = true;
        var constructor = event;
        
        var singleGenericArgument = (genericArgument == null || genericArgument.push == null);
        
        var singleConstraint = (constraint == null || constraint.push == null);

        var eventAggregator = new signalR.EventAggregator(true);


        for (var i = 0; i < 2; i++) {
            if (genericArgument != null) {
                event = {
                    genericConstructor: constructor,
                    genericArguments: [singleGenericArgument ? genericArgument : genericArgument[i]]
                };
            }

            eventAggregator.subscribe(event, function() {}, {}, singleConstraint ? constraint : constraint[i]);
        }

        return function () {
            assert.ok(constraint == null || constructor.__subscribers[0].constraintId != null, "ConstraintId should be set");
            (singleConstraint ? assert.equal : assert.notEqual).bind(assert)(constructor.__subscribers[0].constraintId, constructor.__subscribers[1].constraintId, "ConstraintId should be correct");
        };
    });
};

multipleSameEventSubscriptionTest("When subscribing multiple times to same event - Issue #13");
multipleSameEventSubscriptionTest("When subscribing multiple times to same generic event - Issue #13", "One");
multipleSameEventSubscriptionTest("When subscribing multiple times to same constrained event - Issue #13", null, { id: 1 });
multipleSameEventSubscriptionTest("When subscribing multiple times to event with different constraint - Issue #13", null, [{ id: 1 }, { id: 2 }], 2);
multipleSameEventSubscriptionTest("When subscribing multiple times to event with different generic parameters - Issue #13", ["One", "Two"], null, 2);
multipleSameEventSubscriptionTest("When subscribing multiple times to same constrained generic event - Issue #13", "One", { id: 1 });
multipleSameEventSubscriptionTest("When subscribing multiple times to event with different generic parameters but same constraint", ["One", "Two"], [{ id: 1 }, { id: 1 }], 2);

multipleSameEventUnsubscriptionTest = function (name, genericArgument, constraint, subscribeCount, unsubscribeCount) {
    throttleTest(name, function (assert) {
        subscribeCount = subscribeCount || 2;
        unsubscribeCount = unsubscribeCount || subscribeCount - 1;
        var removeAll = subscribeCount - unsubscribeCount === 0;
        var contexts = [];
        var eventAggregator = new signalR.EventAggregator(true);

        if (removeAll)
            assert.expect(2);

        currentHub.invoke = function (method) {
			switch(method) {
				case "Subscribe":
					for (var i = 0; i < unsubscribeCount; i++) {
						eventAggregator.unsubscribe(contexts[i]);
					}
                    assert.equal(constructor.__subscribers.length, subscribeCount - unsubscribeCount, "Should remove " + unsubscribeCount + " subscriptions client side");				
				break;
				case "Unsubscribe":
                    assert.ok(removeAll, "Should " + (removeAll ? "" : "not") + " unsubscribe");

					return {
						then: function() {
						}
					};
			}				

        };
        
        var event = function () {
        };
        event.proxyEvent = true;
        var constructor = event;

        var singleGenericArgument = (genericArgument == null || genericArgument.push == null);
        var singleConstraint = (constraint == null || constraint.push == null);
        for (var i = 0; i < 2; i++) {
            if (genericArgument != null) {
                event = {
                    genericConstructor: constructor,
                    genericArguments: [singleGenericArgument ? genericArgument : genericArgument[i]]
                };
            }
            var context = {};
            contexts.push(context);
            eventAggregator.subscribe(event, function () { }, context, singleConstraint ? constraint : constraint[i]);
        }
    });
};

multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to same event - Issue #1");
multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to same event and unsubscribing all - Issue #1", null, null, 2, 2);
multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to same generic event - Issue #1", "One", null);
multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to same constrained event - Issue #13", null, { id: 1 });

throttleTest("When two contexts subcribe to different generic arguments of same event", function(assert) {
    assert.expect(0);
    currentHub.invoke = function() {
    };

    var eventOne = constructEvent("One");
    var eventTwo = constructEvent("Two", eventOne);
    
    var eventAggregator = new signalR.EventAggregator(true);
    eventAggregator.subscribe(eventOne.event, function (e) { assert.ok(false, "It should not trigger on wrong generic event"); }, {});

    return function() {
        eventAggregator.publish(new eventOne.constructor(), eventTwo.event.genericArguments);
    };
});

throttleTest("When two contexts subcribe to different generic arguments of same event and unsubscribes", function (assert) {
	currentHub.invoke = function (method, events) {
		switch(method) {
			case "Subscribe":							
			break;
			case "Unsubscribe":
                assert.equal(events.length, 1, "It should only unsubscribe one event")
                assert.equal(events[0].genericArguments[0], "One", "It should unsubscribe event one");

				return {
					then: function() {
					}
				};
		}				

	}; 
    
    var eventOne = constructEvent("One");
    var eventTwo = constructEvent("Two", eventOne);

    var instanceOne = {};
    var instanceTwo = {};

    var eventAggregator = new signalR.EventAggregator(true);
    eventAggregator.subscribe(eventOne.event, function () {}, instanceOne);
    eventAggregator.subscribe(eventTwo.event, function () {}, instanceTwo);

    return function () {
        eventAggregator.unsubscribe(instanceOne);
    };
});

test("When a client is reconnected", function(assert) {
    var startAssert = false;

    var done = assert.async();
    var eventAggregator = signalR.eventAggregator;
	
	currentHub.invoke = function (method, events) {
		switch(method) {
			case "Subscribe":
				eventAggregator.unsubscribe(instanceRemove);
				
				//Wait for unsubscribe
				if(!startAssert) {
                    startAssert = true;
                    window.currentOnClose();
                    return;
                }

                done();
                assert.equal(events.length, 4, "It should resubscribe to the events: " + events.length);			
				
			
			break;
			case "Unsubscribe":


				return {
					then: function (callback) {
						callback();
					}		
				};
		}
	}; 
	

    var eventOne = constructEvent(null, null, true, "EventOne");
    var genericEventOne = constructEvent("Genericarg", null, null, "genericEvent");
    var genericEventTwo = constructEvent("Genericarg2", genericEventOne, true, "genericEvent");
    var eventRemove = constructEvent(null, null, true, "eventRemove");
    var constraintEvent = constructEvent(null, null, true, "constraintEvent");

    var instanceOne = {};
    var instanceTwo = {};
    var instanceRemove = {};
    var thirdConstraintSubscriber = {};

    eventAggregator.subscribe(eventRemove, function () { }, instanceRemove);
    eventAggregator.subscribe(genericEventTwo.event, function () { }, instanceRemove);
    eventAggregator.subscribe(constraintEvent, function () { }, instanceRemove, { foo: 2 });

    eventAggregator.subscribe(eventOne, function() {}, instanceOne);
    eventAggregator.subscribe(genericEventOne.event, function () { }, instanceOne);
    eventAggregator.subscribe(constraintEvent, function () { }, instanceOne, { foo: 1 });

    eventAggregator.subscribe(eventOne, function() {}, instanceTwo);
    eventAggregator.subscribe(genericEventOne.event, function () { }, instanceTwo);
    eventAggregator.subscribe(constraintEvent, function () { }, instanceTwo, { foo: 1 });

    eventAggregator.subscribe(constraintEvent, function () { },thirdConstraintSubscriber, { foo: 3 });
});

QUnit.test("When Hub is missing", function(assert) {
    delete signalR.HubConnectionBuilder;

    try {
        new signalR.EventAggregator(true);
    } catch (error) {
        assert.ok(typeof (error) === "string" && error.indexOf("library") !== -1, "It should throw meaningful exception");
    }
});