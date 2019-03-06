const withSignalREventAggregator = WrappedComponent => {
    return class SignalREventAggregatorComponent extends React.Component {
        constructor(props) {
            super(props);
            this.myRef = React.createRef();
        }

        hookSubscriptions(hook) {
            this.hook = hook;
        }

        componentDidMount() {
            if (this.hook) {
                this.hook(this.subscribe);
            }
        }

        subscribe = (event, handler, constraint) => {
            signalR.eventAggregator.subscribe(event, handler, this.myRef.current, constraint);
        }

        publish = event => {
            signalR.eventAggregator.publish(event);
        }

        componentWillUnmount() {
            signalR.eventAggregator.unsubscribe(this.myRef.current);
        }

        render() {
            return (
                <WrappedComponent
                    {...this.props}
                    hookSubscriptions={this.hookSubscriptions.bind(this)}
                    publish={this.publish}
                    ref={this.myRef}
                />
            );
        }
    };
};