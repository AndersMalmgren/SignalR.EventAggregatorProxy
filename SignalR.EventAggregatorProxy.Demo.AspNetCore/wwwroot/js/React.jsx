class ClientSideEvent {
    constructor(message) {
        this.message = message;
    }
}

class ReactExample extends React.Component {
    constructor() {
        super();
        this.state = { events: [], text: "" };
    }

    componentDidMount() {
        this.props.hookSubscriptions(subscribe => {
            subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.StandardEvent, this.onEvent);
            subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.GenericEvent.of("System.String"), this.onEvent);
            subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.ConstrainedEvent, this.onEvent, { message: "HelloWorld" });
            subscribe(ClientSideEvent, this.onEvent);
        });
    }

    onEvent(e) {
        this.setState(state => ({
            events: state.events.concat(e)
        }));
    }

    handleChange(event) {
        this.setState({ text: event.target.value });
    }

    post(url, data) {
        $.ajax({
            url: url,
            type: 'POST',
            data: ko.toJSON(data),
            contentType: "application/json;charset=utf-8"
        });
    }
    fireStandardEvent() {
        this.post("api/service/fireStandardEvent", this.state.text);
    }
    fireGenericEvent() {
        this.post("api/service/fireGenericEvent", this.state.text);
    }
    fireConstrainedEvent() {
        this.post("api/service/fireConstrainedEvent", this.state.text);
    }
    fireClientSideEvent() {
        this.publish(new ClientSideEvent(this.state.text));
    }

    render() {
        const enabled = this.state.text !== "";

        return (
            <div>
                <h1>Vue demo</h1>

                <label>Message</label> <input type="text" value={this.state.text} onChange={this.handleChange.bind(this)} />
                <div>
                    <button disabled={!enabled} onClick={this.fireStandardEvent.bind(this)}>Fire standard event</button>
                    <button disabled={!enabled} onClick={this.fireGenericEvent.bind(this)}>Fire generic event</button>
                    <button disabled={!enabled} onClick={this.fireConstrainedEvent.bind(this)}>Fire constrained event (Message must be 'HelloWorld' for the event to fire on this client</button>
                    <button disabled={!enabled} onClick={this.fireClientSideEvent.bind(this)}>Fire client side event</button>
                </div>

                <h3>Events</h3>
                {this.state.events.map((e, index) => <div key={index}>{JSON.stringify(e)}</div>) }
            </div>
        );
    }
}

const SignalrReactExample = withSignalREventAggregator(ReactExample);

ReactDOM.render(<SignalrReactExample />, document.getElementById('app'));