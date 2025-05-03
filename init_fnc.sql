create or replace function prevent_duplicate_chat_members()
returns trigger as $$
begin
    if exists (
        select 1 from chat_members
        where chat_id = new.chat_id and user_id = new.user_id
    ) then
        raise exception 'пользователь уже добавлен в чат';
    end if;
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