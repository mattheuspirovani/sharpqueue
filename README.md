# SharpQueue 

SharpQueue is a minimalist messaging service developed in C#, designed to provide a lightweight queueing system with real-time notifications. It is ideal for scenarios that do not require handling millions of requests. This project uses Docker containerization to facilitate deployment and scalability.

## Features

- **Queue System**: Create and manage queues for message exchanges.
- **Real-Time Notifications**: Notifies subscribed consumers in a specific queue when a new message is added.
- **Lightweight & Efficient**: Designed to work as a sidecar service for lightweight message processing.

## Project Structure

- **SharpQueue.NotificationService**: Main project for managing client connections, queue subscriptions, and message handling.
- **SharpQueue.Core**: Core library containing interfaces and models for message processing and queue notifications.


## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Submit a pull request with a detailed description of your changes.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

For questions or suggestions, please contact [mattheus.pirovani@gmail.com]