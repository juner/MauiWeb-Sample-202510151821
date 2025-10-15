import fs from "node:fs";

type Stats = fs.Stats;
const dirs = [
  // 出力 ディレクトリ
  "./dist",
  // donet 中間ファイル ディレクトリ
  "./obj",
];
for (const dir of dirs) await rmdir(dir);
/**
 * 指定したパスのディレクトリを削除する
 * @param path
 * @returns
 */
async function rmdir(path: string) {
  try {
    if (!fs.existsSync(path)) {
      console.log(`not found ${path} skip.`);
      return 1;
    }
    const dir = await (() => {
      const { resolve, reject, promise } = Promise.withResolvers<Stats>();
      fs.stat(path, (err, stats) => {
        if (err) {
          reject(err);
          return;
        }
        resolve(stats);
      });
      return promise;
    })();
    if (!dir.isDirectory()) {
      console.log(`${path} is not directory.`);
      return 2;
    }
    await (() => {
      const { resolve, reject, promise } = Promise.withResolvers<void>();
      fs.rm(path, { recursive: true, force: true }, (err) => {
        if (err) {
          reject(err);
          return;
        }
        resolve();
      });
      return promise;
    })();
    console.log(`complete ${path} removed.`);
    return 0;
  } catch (e: unknown) {
    console.log(e);
  }
}
