> [!NOTE]
> If you want to test this controller, you need install `Postman` extension.

# Test controller

1. Open Postman extension and create new HTTP request.
2. Insert link like `http://localhost:5197/{uuid}`

> [!TIP]
> Please replace `{uuid}` to real uuid.
> You can get uuids here -> [IT Tools](https://it-tools.tech/uuid-generator)

3. Switch METHOD to `POST`.
4. Go to Postman `Body` tab, insert file to form-data.
5. Send request.
6. Open `test/index.html`
7. Edit image link in html file.
8. Switch METHOD to `DELETE`.
9. Send request.
