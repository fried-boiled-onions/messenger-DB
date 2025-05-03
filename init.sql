create table if not exists users (
    id serial primary key,
    username varchar(50) not null unique, --имя пользователя
    email varchar(100) unique, --электронная почта
    password_hash varchar(255) not null, --хэшированный пароль
    created_at timestamp default now() --дата регистрации
);

create table if not exists chats (
    id serial primary key,
    name varchar(100), -- название чата
    is_group boolean default false, --является ли чат групповым
    created_at timestamp default now() --дата и время создания чата
);

create table if not exists chat_members (
    id serial primary key,
    chat_id int references chats(id) on delete cascade, --айди чата
    user_id int references users(id) on delete cascade, --айди пользователей
    joined_at timestamp default now(), -- дата и время добавления пользователя в чат
    foreign key (chat_id) references chats (id) on delete cascade,
    foreign key (user_id) references users (id) on delete cascade
);

create table if not exists messages (
    id serial primary key,
    chat_id  int references chats(id) on delete cascade, --идентификатор чата
    sender_id int references users(id) on delete set null, --идентификатор отправителя
    content text not null, --содержание сообщения
    timestamp timestamp default now(), --время отправления
    is_read boolean default false, --индикатор прочитано сообщение или нет
    foreign key (chat_id) references chats (id) on delete cascade,
    foreign key (sender_id) references users (id) on delete cascade
)

create table if not exists notifications (
    id serial primary key,
    user_id int references users(id) on delete cascade, --идентификатор пользователя
    type varchar(50) not null, --тип уведомления
    data jsonb, --содержание уведомления
    created_at timestamp default now(), --дата и время уведомления
    is_seen boolean default false, --индикатор прочитано сообщение или нет
    foreign key (user_id) references users (id) on delete cascade
);

drop table users cascade;
drop table chats cascade;
drop table chat_members cascade;
drop table messages cascade;
drop table notifications cascade;