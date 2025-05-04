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
);

create table if not exists notifications (
    id serial primary key,
    user_id int references users(id) on delete cascade, --идентификатор пользователя
    type varchar(50) not null, --тип уведомления
    data jsonb, --содержание уведомления
    created_at timestamp default now(), --дата и время уведомления
    is_seen boolean default false, --индикатор прочитано сообщение или нет
    foreign key (user_id) references users (id) on delete cascade
);

create or replace function prevent_duplicate_chat_members()
returns trigger as $$
begin
    if exists (
        select 1 from chat_members
        where chat_id = new.chat_id and user_id = new.user_id
    ) then
        raise exception 'пользователь уже добавлен в чат';
    end if;

    return new;
end;
$$ language plpgsql;

create trigger check_duplicate_members
before insert on chat_members
for each row
execute function prevent_duplicate_chat_members();


create or replace function delete_user(usr_id int)
returns void as $$
begin
    delete from messages where sender_id = usr_id;
    delete from chat_members where user_id = usr_id;
    delete from users where id = usr_id;
end;
$$ language plpgsql;

create or replace function get_chat_member_count(chat int)
returns integer as $$
begin
    return (select count(*) from chat_members where chat_id = chat);
end;
$$ language plpgsql;


create or replace function send_message(sender int, chat int, message_content text)
returns int as $$
declare
    message_id int;
begin
    insert into messages (chat_id, sender_id, content, timestamp)
    values (chat, sender, content, current_timestamp)
    returning id into message_id;
    return message_id;
end;
$$ language plpgsql;


create index idx_chat_members_user_id on chat_members (user_id);
create index idx_messages_chat_id on messages (chat_id);

create or replace function create_user(p_username varchar, p_email varchar, p_password_hash varchar)
returns int as $$
declare
    new_user_id int;
begin
    insert into users (username, email, password_hash)
    values (p_username, p_email, p_password_hash)
    returning id into new_user_id;

    return new_user_id;
    exception
        when unique_violation then
            raise exception 'user with email % or username % already exists', p_email, p_username;
end;
$$ language plpgsql;

create or replace function update_user(user_id int, new_username varchar)
returns void as $$
begin
    update users
    set username = new_username
    where id = user_id;

    if not found then
        raise exception 'user with id % does not exist', user_id;
    end if;

    exception
        when unique_violation then
            raise exception 'duplicate username % already exists', new_username;
end;
$$ language plpgsql;