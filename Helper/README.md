docker 
// Problems to solve -
// как ограничить данные в модели. перед сохранением их в бд, сохранять както ИД.
// и задавая вопросы, уточнять что ответы нужны только по нужным ИД
// при миграции как их убивать
// ИЛИ хранить данные в векторной БД, и там вся магия.


https://github.com/ollama/ollama

-- docker run -d -v ollama_data:/root/.ollama -p 11434:11434 --name ollama ollama/ollama:latest
-- docker exec -it ollama ollama pull llama3


-- docker run -d --cpus="2.0" -v ollama_data:/root/.ollama -p 11434:11434 --name ollama ollama/ollama:latest 
-- docker exec -it ollama ollama pull llama3.2

-- docker exec -t ollama ollama list all models
