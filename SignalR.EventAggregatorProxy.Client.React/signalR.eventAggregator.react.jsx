const withSignalREventAggregator = WrappedComponent => {
    return class SignalREventAggregatorComponent extends React.Component {
        constructor(props) {
            super(props);
            this.myRef = React.createRef();
            this.queuedSubscriptions = [];
        }

        componentDidMount() {
            if (this.queuedSubscriptions.length > 0) {
                this.queuedSubscriptions.forEach(s => {
                    this.subscribe(s.event, s.handler, s.constraint);
                });
                this.queuedSubscriptions = null;  //Only support deferred subscriptions at mount time. 
            }
        }

        subscribe = (event, handler, constraint) => {
            if (!this.myRef.current) {
                if (!this.queuedSubscriptions) throw "Context scope for subscription could not be resolved";

                this.queuedSubscriptions.push({ event, handler, constraint });
                return;
            }

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
                    subscribe={this.subscribe}
                    publish={this.publish}
                    ref={this.myRef}
                />
            );
        }
    };
};