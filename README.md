# Banking Console App
### RMIT Web Development Technologies (COSC2277), Flexible Term 2020, assignment 1
## Group Info
    Shiyu Gao, s3734720

## Design Patterns Used:
### Facade
        In general, the facade pattern offers a simplified interface for complex systems. It is used when several complex systems need to interact in a certain way to perform the functionality we want, or when we only need to use a subset of the functionalities the complex systems offer, and in a certain way.

        We used the facade pattern to offer a relatively simple interface in the Driver class, which offers all functionalities to the UI/console. We used facade to hide from the UI the details of implementation, and to sum up all the functionalities provided by separated Manager classes.

        We also used facade where account manager need both account manager implementation class and transaction manager implementation class for operations such as transfer. In this case, we only need a subset of the functionalities of both classes and in a certain coordinated way. We used facade to enforce parameter checks, to coordinate the functionalities, and to tranform the return values.

### Dependency Injection
        In general, the DI pattern offers a neater way to supply dependencies. It hides the detail of creation of the dependency objects. It also hides implementation detail/members of the dependency by first designing an interface and using only that interface.

        We used dependency injection to inject the functionalities of the implementation classes to the manager classes, so as to hide the implementation details and various members of the implementation classes from the manager classes. Also, by using DI, we freed the manager classes from the task of creating instances of the implementation classes, and having to store low-level parameters like the connection string.
