# Recipe Website (Back-end)
## Введение

Добро пожаловать на это веб-приложение, созданное для упрощения поиска, создания и обмена рецептами. Целью моего проекта является предоставление удобного интерфейса для пользователей, где они могут не только находить интересные рецепты, но и делиться своими кулинарными шедеврами с другими. В этой документации будет охарактеризована серверная часть приложения, включая информацию о функциональности, архитектуре и используемых API эндпоинтах.

## Эндпоинты API

В нашем приложении реализованы следующие эндпоинты, которые позволяют пользователям взаимодействовать с ресурсами системы. Ниже представлена таблица с методами запросов, путями до эндпоинтов и кратким описанием каждого из них.

| Метод  | Путь                       | Описание                                                                                            |
|--------|----------------------------|-----------------------------------------------------------------------------------------------------|
| POST   | /api/users/login           | Аутентификация пользователя по имени и паролю. Возвращает JWT-токен для последующей авторизации.    |
| POST   | /api/users/signup          | Регистрация нового пользователя с именем и паролем. Возвращает JWT-токен для авторизации.           |
| PUT    | /api/users                 | Обновление пароля пользователя. Требует JWT-токен, старый и новый пароль передаются в теле запроса. |
| GET    | /api/users/{:id}           | Получение данных о пользователе по его ID.                                                          |
| ---    | ---                        | ---                                                                                                 |
| POST   | /api/recipes               | Создание нового рецепта (доступно только авторизованным пользователям).                             |
| POST   | /api/recipes/{:id}/rate    | Оценка рецепта от 1 до 5 (доступно только авторизованным пользователям).                            |
| POST   | /api/recipes/{:id}/comment | Добавление комментария к рецепту (доступно только авторизованным пользователям).                    |
| GET    | /api/recipes/{:id}         | Поиск рецепта по его ID.                                                                            |
| GET    | /api/recipes/page          | Пагинация рецептов с возможностью сортировки.                                                       |
| GET    | /api/recipes/search        | Поиск рецептов по ключевому слову.                                                                  |
| PUT    | /api/recipes/{:id}         | Изменение данных рецепта (доступно только его создателю).                                           |
| DELETE | /api/recipes/{:id}         | Удаление рецепта (доступно только создателю рецепта или пользователю администратору).               |

## Описание запросов эндпоинтов:

## POST /api/users/login & POST /api/users/signup
```json
{
  "username": "string",
  "password": "string"
}
```
<hr />

## PUT /api/users
```json
{
  "oldPassword": "string",
  "newPassword": "string"
}
```
<hr />

## <b>POST</b> /api/recipes/

<b>ACCEPTS: multipart/form-data</b>
```
Title: string
Description: string
Instruction: string
Difficulty: string
CookingTime: string
Ingredients: [
    {
        name: string,
        count: numeric,
        unitType: string
    }
]
```
<hr />

## <b>POST</b> /api/recipes/{:id}/rate

<b>ACCEPTS: application/x-www-form-urlencoded
```
stars: int
```
<hr />

## <b>POST</b> /api/recipes/{:id}/comment

<b>ACCEPTS: application/x-www-form-urlencoded</b>
```
content: string
```
<hr />

## <b>GET</b> /api/recipes/page

Params:
- page: int
- pageSize: int
- sortType: string
<hr />

## <b>GET</b> /api/recipes/search

Params:
- query: string
<hr />

## <b>PUT</b> /api/recipes/{:id}

<b>ACCEPTS: multipart/form-data</b>
```
Title: string?
Description: string?
Instruction: string?
Difficulty: string?
CookingTime: string?
Ingredients: array? [
    {
        name: string,
        count: numeric,
        unitType: string
    }
]
```













