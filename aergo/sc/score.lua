--
-- Space Shooter
--

MODULE_NAME = "__SPACE_SHOOTER__"

MODULE_NAME_DB = "__MANIFEST_DB__"

state.var {
  -- contant variables
  _MANIFEST_ADDRESS = state.value(),
}

local function __init__(manifestAddress)
  _MANIFEST_ADDRESS:set(manifestAddress)
  local scAddress = system.getContractID()
  system.print(MODULE_NAME .. "__init__: sc_address=" .. scAddress)
  contract.call(_MANIFEST_ADDRESS:get(),
    "__init_module__", MODULE_NAME, scAddress)
end

local function __callFunction(module_name, func_name, ...)
  system.print(MODULE_NAME .. "__callFucntion: module_name=" .. module_name
          .. ", func_name=" .. func_name)
  return contract.call(_MANIFEST_ADDRESS:get(),
    "__call_module_function__", module_name, func_name, ...)
end

local function __getManifestAddress()
  local address = _MANIFEST_ADDRESS:get()
  system.print(MODULE_NAME .. "__getManifestAddress: address=" .. address) 
  return address
end

--[[ ====================================================================== ]]--

function constructor(manifestAddress)
  __init__(manifestAddress)
  system.print(MODULE_NAME
          .. "constructor: manifestAddress=" .. manifestAddress)
 
  -- create user table
  __callFunction(MODULE_NAME_DB, "createTable",
    [[CREATE TABLE IF NOT EXISTS SpaceShooter(
            user_id    TEXT NOT NULL,
            score      INTEGER DEFAULT 0,
            playtime   TEXT NOT NULL,
            block_no   INTEGER DEFAULT NULL,
            tx_id      TEXT NOT NULL,
            PRIMARY KEY(tx_id)
  )]])
end

local function isEmpty(v)
  return nil == v or 0 == string.len(v)
end

function addScore(user_id, score, playtime)
  system.print(MODULE_NAME .. "addScore: user_id=" .. tostring(user_id)
          .. ", score=" .. tostring(score) 
	  .. ", playtime=" .. tostring(playtime))

  local sender = system.getOrigin()
  local block_no = system.getBlockheight()
  system.print(MODULE_NAME .. "addScore: sender=" .. sender
          .. ", block_no=" .. block_no)

  -- tx id
  local tx_id = system.getTxhash()
  system.print(MODULE_NAME .. "addScore: tx_id=" .. tx_id)

  -- insert a new user
  __callFunction(MODULE_NAME_DB, "insert",
    [[INSERT INTO SpaceShooter(user_id,
                               score,
			       playtime,
			       block_no,
			       tx_id)
             VALUES (?, ?, ?, ?, ?)]],
    user_id, score, playtime, block_no, tx_id)

  -- success to write (201 Created)
  return {
    __module = MODULE_NAME,
    __block_no = block_no,
    __func_name = "addScore",
    __status_code = "201",
    __status_sub_code = "",
    user_id = user_id,
    score = score,
    playtime = playtime,
    block_no = block_no,
    tx_id = tx_id,
  }
end

function getTopScores(top)
  system.print(MODULE_NAME .. "getTopScore: top=" .. tostring(top))

  if nil == top then
    top = 10
  end

  local rows = __callFunction(MODULE_NAME_DB, "select",
    [[SELECT user_id, score, playtime, block_no, tx_id FROM SpaceShooter 
        ORDER BY score DESC LIMIT ]] .. top)
  local score_list = {}
  local exist = false
  for _, v in pairs(rows) do
    table.insert(score_list, {
      user_id = v[1],
      score = v[2],
      playtime = v[3],
      block_no = v[4],
      tx_id = v[5],
    })
    exist = true
  end

  -- if not exist, (404 Not Found)
  if not exist then
    return {
      __module = MODULE_NAME,
      __func_name = "getTopScore",
      __status_code = "404",
      __status_sub_code = "",
      __err_msg = "cannot find any score",
    }
  end

  return {
    __module = MODULE_NAME,
    __func_name = "getTopScore",
    __status_code = "200",
    __status_sub_code = "",
    score_list = score_list,
  }
end

abi.register(addScore, getTopScores)
