# autenticadorBoleto
Projeto autenticador de boleto

Neste projeto foi criado um serviço de barramento fila para receber as mensagens utilizando Azure Functions:
![image](https://github.com/user-attachments/assets/d727b3e0-e3db-401f-a1fd-8fdadf45d9e2)

Versão do dotNet utilizada:
- 4.8

Para criar um projeto do tipo Azure Functions, foi necessário adicionar as seguintes depnedências:
- Newtonsoft.Json
- BarcodeLib
- Azure.Messaging.ServiceBus

Após implementar a aplicação e subir, será apresentada uma tela de prompt de comando com a URL de chamada da fila:
![image](https://github.com/user-attachments/assets/9573ba58-cc9c-4ae8-b40f-049fdb1c3fbf)

Ao realizar a chamada da fila localmente, é apresentado o retorno: Welcome to Azure Functions!
![image](https://github.com/user-attachments/assets/2730cf6e-e901-4124-87f0-930c344e8167)

Aqui está o objetos enviado para a fila na Azure:
![image](https://github.com/user-attachments/assets/fb720cd4-c72b-45cf-a5d3-1d48cac3636f)

Código de barras do Boleto gerado:
![image](https://github.com/user-attachments/assets/f27f8408-9188-4e9a-9801-18d959a8f5cb)

![image](https://github.com/user-attachments/assets/3eaa15a1-ff10-4866-aca7-333164cc59da)





