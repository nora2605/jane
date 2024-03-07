mod byte_code;

pub enum OpCode {
    Halt,
    Constant(i32),
    Add
}

pub fn make() -> Vec<u8> {
    todo!();
}